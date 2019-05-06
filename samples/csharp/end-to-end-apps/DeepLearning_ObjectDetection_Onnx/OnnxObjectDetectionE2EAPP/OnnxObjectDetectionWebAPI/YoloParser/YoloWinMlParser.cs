using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OnnxObjectDetectionWebAPI
{
    class YoloWinMlParser
    {
        public const int ROW_COUNT = 13;
        public const int COL_COUNT = 13;
        public const int CHANNEL_COUNT = 125;
        public const int BOXES_PER_CELL = 5;
        public const int BOX_INFO_FEATURE_COUNT = 5;
        public const int CLASS_COUNT = 20;
        public const float CELL_WIDTH = 32;
        public const float CELL_HEIGHT = 32;

        private int channelStride = ROW_COUNT * COL_COUNT;

        private float[] anchors = new float[]
            {
                1.08F, 1.19F, 3.42F, 4.41F, 6.63F, 11.38F, 9.42F, 5.11F, 16.62F, 10.52F
            };

        private string[] labels = new string[]
            {
                "aeroplane", "bicycle", "bird", "boat", "bottle",
                "bus", "car", "cat", "chair", "cow",
                "diningtable", "dog", "horse", "motorbike", "person",
                "pottedplant", "sheep", "sofa", "train", "tvmonitor"
            };

        public IList<YoloBoundingBox> ParseOutputs(float[] yoloModelOutputs, float threshold = .3F)
        {
            var boxes = new List<YoloBoundingBox>();

            var featuresPerBox = BOX_INFO_FEATURE_COUNT + CLASS_COUNT;
            var stride = featuresPerBox * BOXES_PER_CELL;

            for (int cy = 0; cy < ROW_COUNT; cy++)
            {
                for (int cx = 0; cx < COL_COUNT; cx++)
                {
                    for (int b = 0; b < BOXES_PER_CELL; b++)
                    {
                        var channel = (b * (CLASS_COUNT + BOX_INFO_FEATURE_COUNT));

                        var tx = yoloModelOutputs[GetOffset(cx, cy, channel)];
                        var ty = yoloModelOutputs[GetOffset(cx, cy, channel + 1)];
                        var tw = yoloModelOutputs[GetOffset(cx, cy, channel + 2)];
                        var th = yoloModelOutputs[GetOffset(cx, cy, channel + 3)];
                        var tc = yoloModelOutputs[GetOffset(cx, cy, channel + 4)];

                        var x = ((float)cx + Sigmoid(tx)) * CELL_WIDTH;
                        var y = ((float)cy + Sigmoid(ty)) * CELL_HEIGHT;
                        var width = (float)Math.Exp(tw) * CELL_WIDTH * this.anchors[b * 2];
                        var height = (float)Math.Exp(th) * CELL_HEIGHT * this.anchors[b * 2 + 1];

                        var confidence = Sigmoid(tc);

                        if (confidence < threshold)
                            continue;

                        var classes = new float[CLASS_COUNT];
                        var classOffset = channel + BOX_INFO_FEATURE_COUNT;

                        for (int i = 0; i < CLASS_COUNT; i++)
                            classes[i] = yoloModelOutputs[GetOffset(cx, cy, i + classOffset)];

                        var results = Softmax(classes)
                            .Select((v, ik) => new { Value = v, Index = ik });

                        var topClass = results.OrderByDescending(r => r.Value).First().Index;
                        var topScore = results.OrderByDescending(r => r.Value).First().Value * confidence;
                        var testSum = results.Sum(r => r.Value);

                        if (topScore < threshold)
                            continue;

                        boxes.Add(new YoloBoundingBox()
                        {
                            Confidence = topScore,
                            X = (x - width / 2),
                            Y = (y - height / 2),
                            Width = width,
                            Height = height,
                            Label = this.labels[topClass]
                        });
                    }
                }
            }

            return boxes;
        }

        public List<YoloBoundingBox> NonMaxSuppress(IList<YoloBoundingBox> boxes, int limit, float threshold)
        {
            var activeCount = boxes.Count;
            var isActiveBoxes = new bool[boxes.Count];

            for (int i = 0; i < isActiveBoxes.Length; i++)
                isActiveBoxes[i] = true;

            var sortedBoxes = boxes.Select((b, i) => new { Box = b, Index = i })
                                .OrderByDescending(b => b.Box.Confidence)
                                .ToList();

            var results = new List<YoloBoundingBox>();

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

        private float IntersectionOverUnion(RectangleF a, RectangleF b)
        {
            var areaA = a.Width * a.Height;

            if (areaA <= 0)
                return 0;

            var areaB = b.Width * b.Height;

            if (areaB <= 0)
                return 0;

            var minX = Math.Max(a.Left, b.Left);
            var minY = Math.Max(a.Top, b.Top);
            var maxX = Math.Min(a.Right, b.Right);
            var maxY = Math.Min(a.Bottom, b.Bottom);

            var intersectionArea = Math.Max(maxY - minY, 0) * Math.Max(maxX - minX, 0);

            return intersectionArea / (areaA + areaB - intersectionArea);
        }

        private int GetOffset(int x, int y, int channel)
        {
            // YOLO outputs a tensor that has a shape of 125x13x13, which 
            // WinML flattens into a 1D array.  To access a specific channel 
            // for a given (x,y) cell position, we need to calculate an offset
            // into the array
            return (channel * this.channelStride) + (y * COL_COUNT) + x;
        }

        private float Sigmoid(float value)
        {
            var k = (float)Math.Exp(value);

            return k / (1.0f + k);
        }

        private float[] Softmax(float[] values)
        {
            var maxVal = values.Max();
            var exp = values.Select(v => Math.Exp(v - maxVal));
            var sumExp = exp.Sum();

            return exp.Select(v => (float)(v / sumExp)).ToArray();
        }
    }
}