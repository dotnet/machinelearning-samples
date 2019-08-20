using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OnnxObjectDetection
{
    public class OnnxOutputParser
    {
        class CellDimensions : DimensionsBase { }
        
        // The number of rows in the grid the image is divided into.
        public const int ROW_COUNT = 13;
        
        // The number of columns in the grid the image is divided into.
        public const int COL_COUNT = 13;

        // The total number of values contained in one cell of the grid.
        // CHANNEL_COUNT = (CLASS_COUNT + BOX_INFO_FEATURE_COUNT) * BOXES_PER_CELL
        // public const int CHANNEL_COUNT = 125;

        // The number of bounding boxes in a cell.
        public const int BOXES_PER_CELL = 5;

        // The number of features contained within a box (x,y,height,width,confidence).
        public const int BOX_INFO_FEATURE_COUNT = 5;

        // The starting position of the current cell in the grid.
        private readonly int channelStride = ROW_COUNT * COL_COUNT;

        // The width of one cell in the image grid.
        // i.e. input image width (416) / COL_COUNT = 32
        private readonly float cellWidth = ImageSettings.imageWidth / COL_COUNT;

        // The height of one cell in the image grid.
        // i.e. input image height (416) / ROW_COUNT = 32
        private readonly float cellHeight = ImageSettings.imageHeight / ROW_COUNT;

        // The number of class predictions contained in each bounding box.
        // For example if your object detection model can detect 11 different
        // objects, then the classCount = 11
        private readonly int classCount;

        private readonly float[] anchors;
        private readonly string[] labels;

        private static readonly Color[] classColors = new Color[]
        {
            Color.Khaki, Color.Fuchsia, Color.Silver, Color.RoyalBlue,
            Color.Green, Color.DarkOrange, Color.Purple, Color.Gold,
            Color.Red, Color.Aquamarine, Color.Lime, Color.AliceBlue,
            Color.Sienna, Color.Orchid, Color.Tan, Color.LightPink,
            Color.Yellow, Color.HotPink, Color.OliveDrab, Color.SandyBrown,
            Color.DarkTurquoise
        };

        public OnnxOutputParser(IOnnxModel onnxModel)
        {
            labels = onnxModel.Labels;
            anchors = onnxModel.Anchors;
            classCount = onnxModel.Labels.Length;
        }

        // Applies the sigmoid function that outputs a number between 0 and 1.
        private float Sigmoid(float value)
        {
            var k = (float)Math.Exp(value);
            return k / (1.0f + k);
        }

        // Normalizes an input vector into a probability distribution.
        private float[] Softmax(float[] values)
        {
            var maxVal = values.Max();
            var exp = values.Select(v => Math.Exp(v - maxVal));
            var sumExp = exp.Sum();

            return exp.Select(v => (float)(v / sumExp)).ToArray();
        }

        // Maps elements in the one-dimensional model output to the corresponding position in a 125 x 13 x 13 tensor.
        private int GetOffset(int x, int y, int channel)
        {
            // Onnx outputs a tensor that has a shape of 125x13x13, which 
            // WinML flattens into a 1D array.  To access a specific channel 
            // for a given (x,y) cell position, we need to calculate an offset
            // into the array
            return (channel * channelStride) + (y * COL_COUNT) + x;
        }

        // Extracts the bounding box dimensions using the GetOffset method from the model output.
        private BoundingBoxDimensions ExtractBoundingBoxDimensions(float[] modelOutput, int x, int y, int channel)
        {
            return new BoundingBoxDimensions
            {
                X = modelOutput[GetOffset(x, y, channel)],
                Y = modelOutput[GetOffset(x, y, channel + 1)],
                Width = modelOutput[GetOffset(x, y, channel + 2)],
                Height = modelOutput[GetOffset(x, y, channel + 3)]
            };
        }

        // Extracts the confidence value which states how sure the model is that it has detected an object 
        // and uses the Sigmoid function to turn it into a percentage.
        private float GetConfidence(float[] modelOutput, int x, int y, int channel)
        {
            return Sigmoid(modelOutput[GetOffset(x, y, channel + 4)]);
        }

        // Uses the bounding box dimensions and maps them onto its respective cell within the image.
        private CellDimensions MapBoundingBoxToCell(int x, int y, int box, BoundingBoxDimensions boxDimensions)
        {
            return new CellDimensions
            {
                X = (x + Sigmoid(boxDimensions.X)) * cellWidth,
                Y = (y + Sigmoid(boxDimensions.Y)) * cellHeight,
                Width = (float)Math.Exp(boxDimensions.Width) * cellWidth * anchors[box * 2],
                Height = (float)Math.Exp(boxDimensions.Height) * cellHeight * anchors[box * 2 + 1],
            };
        }

        // Extracts the class predictions for the bounding box from the model output using the GetOffset 
        // method and turns them into a probability distribution using the Softmax method.
        public float[] ExtractClasses(float[] modelOutput, int x, int y, int channel)
        {
            float[] predictedClasses = new float[classCount];
            int predictedClassOffset = channel + BOX_INFO_FEATURE_COUNT;
            for (int predictedClass = 0; predictedClass < classCount; predictedClass++)
            {
                predictedClasses[predictedClass] = modelOutput[GetOffset(x, y, predictedClass + predictedClassOffset)];
            }
            return Softmax(predictedClasses);
        }

        // Selects the class from the list of predicted classes with the highest probability.
        private ValueTuple<int, float> GetTopResult(float[] predictedClasses)
        {
            return predictedClasses
                .Select((predictedClass, index) => (Index: index, Value: predictedClass))
                .OrderByDescending(result => result.Value)
                .First();
        }

        // Filters overlapping bounding boxes with lower probabilities.
        private float IntersectionOverUnion(RectangleF boundingBoxA, RectangleF boundingBoxB)
        {
            var areaA = boundingBoxA.Width * boundingBoxA.Height;

            if (areaA <= 0)
                return 0;

            var areaB = boundingBoxB.Width * boundingBoxB.Height;

            if (areaB <= 0)
                return 0;

            var minX = Math.Max(boundingBoxA.Left, boundingBoxB.Left);
            var minY = Math.Max(boundingBoxA.Top, boundingBoxB.Top);
            var maxX = Math.Min(boundingBoxA.Right, boundingBoxB.Right);
            var maxY = Math.Min(boundingBoxA.Bottom, boundingBoxB.Bottom);

            var intersectionArea = Math.Max(maxY - minY, 0) * Math.Max(maxX - minX, 0);

            return intersectionArea / (areaA + areaB - intersectionArea);
        }

        public IList<BoundingBox> ParseOutputs(float[] yoloModelOutputs, float threshold = .3f)
        {
            var boxes = new List<BoundingBox>();

            for (int row = 0; row < ROW_COUNT; row++)
            {
                for (int column = 0; column < COL_COUNT; column++)
                {
                    for (int box = 0; box < BOXES_PER_CELL; box++)
                    {
                        var channel = (box * (classCount + BOX_INFO_FEATURE_COUNT));

                        BoundingBoxDimensions boundingBoxDimensions = ExtractBoundingBoxDimensions(yoloModelOutputs, row, column, channel);

                        float confidence = GetConfidence(yoloModelOutputs, row, column, channel);

                        CellDimensions mappedBoundingBox = MapBoundingBoxToCell(row, column, box, boundingBoxDimensions);

                        if (confidence < threshold)
                            continue;

                        float[] predictedClasses = ExtractClasses(yoloModelOutputs, row, column, channel);

                        var (topResultIndex, topResultScore) = GetTopResult(predictedClasses);
                        var topScore = topResultScore * confidence;

                        if (topScore < threshold)
                            continue;

                        boxes.Add(new BoundingBox()
                        {
                            Dimensions = new BoundingBoxDimensions
                            {
                                X = (mappedBoundingBox.X - mappedBoundingBox.Width / 2),
                                Y = (mappedBoundingBox.Y - mappedBoundingBox.Height / 2),
                                Width = mappedBoundingBox.Width,
                                Height = mappedBoundingBox.Height,
                            },
                            Confidence = topScore,
                            Label = labels[topResultIndex],
                            BoxColor = topResultIndex < classColors.Length ? classColors[topResultIndex] : classColors[topResultIndex % classColors.Length]
                        });
                    }
                }
            }
            return boxes;
        }

        public IList<BoundingBox> FilterBoundingBoxes(IList<BoundingBox> boxes, int limit, float threshold)
        {
            var activeCount = boxes.Count;
            var isActiveBoxes = new bool[boxes.Count];

            for (int i = 0; i < isActiveBoxes.Length; i++)
                isActiveBoxes[i] = true;

            var sortedBoxes = boxes.Select((b, i) => new { Box = b, Index = i })
                                .OrderByDescending(b => b.Box.Confidence)
                                .ToList();

            var results = new List<BoundingBox>();

            for (int i = 0; i < boxes.Count; i++)
            {
                if (isActiveBoxes[i])
                {
                    var boxA = sortedBoxes[i].Box;
                    results.Add(boxA);

                    if (results.Count >= limit)
                        break;

                    for (var j = i + 1; j < boxes.Count; j++)
                    {
                        if (isActiveBoxes[j])
                        {
                            var boxB = sortedBoxes[j].Box;

                            if (IntersectionOverUnion(boxA.Rect, boxB.Rect) > threshold)
                            {
                                isActiveBoxes[j] = false;
                                activeCount--;

                                if (activeCount <= 0)
                                    break;
                            }
                        }
                    }

                    if (activeCount <= 0)
                        break;
                }
            }
            return results;
        }
    }
}