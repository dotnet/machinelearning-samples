using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Microsoft.ML;

namespace ObjectDetection
{
    class OnnxModelScorer
    {
        private readonly string imagesFolder;
        private readonly string modelLocation;
        private readonly MLContext mlContext;

        private IList<YoloBoundingBox> _boundingBoxes = new List<YoloBoundingBox>();
        private readonly YoloWinMlParser _parser = new YoloWinMlParser();

        public OnnxModelScorer(string imagesFolder, string modelLocation)
        {
            this.imagesFolder = imagesFolder;
            this.modelLocation = modelLocation;
            mlContext = new MLContext();
        }

        public struct ImageNetSettings
        {
            public const int imageHeight = 416;
            public const int imageWidth = 416;
        }

        public struct TinyYoloModelSettings
        {
            // for checking TIny yolo2 Model input and  output  parameter names,
            //you can use tools like Netron, 
            // which is installed by Visual Studio AI Tools

            // input tensor name
            public const string ModelInput = "image";

            // output tensor name
            public const string ModelOutput = "grid";
        }

        public void Score()
        {
            var model = LoadModel(modelLocation);

            PredictDataUsingModel(imagesFolder, model);
        }

        private PredictionEngine<ImageNetData, ImageNetPrediction> LoadModel(string modelLocation)
        {
            Console.WriteLine("Read model");
            Console.WriteLine($"Model location: {modelLocation}");
            Console.WriteLine($"Default parameters: image size=({ImageNetSettings.imageWidth},{ImageNetSettings.imageHeight})");

            var data = CreateEmptyDataView();

            var pipeline = mlContext.Transforms.LoadImages(outputColumnName: "image", imageFolder: "", inputColumnName: nameof(ImageNetData.ImagePath))
                            .Append(mlContext.Transforms.ResizeImages(outputColumnName: "image", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: "image"))
                            .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "image"))
                            .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { TinyYoloModelSettings.ModelOutput }, inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));

            var model = pipeline.Fit(data);

            var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageNetData, ImageNetPrediction>(model);

            return predictionEngine;
        }

        protected void PredictDataUsingModel(string imagesFolder, PredictionEngine<ImageNetData, ImageNetPrediction> model)
        {
            Console.WriteLine($"Images location: {imagesFolder}");
            Console.WriteLine("");
            Console.WriteLine("=====Identify the objects in the images=====");
            Console.WriteLine("");

            var testData = GetImagesData(imagesFolder);

            foreach (var sample in testData)
            {
                var probs = model.Predict(sample).PredictedLabels;
                _boundingBoxes = _parser.ParseOutputs(probs);
                var filteredBoxes = _parser.FilterBoundingBoxes(_boundingBoxes, 5, .5F);


                var outputDirectory = Path.Combine(Directory.GetParent(sample.ImagePath).FullName, "output");
                var filename = new FileInfo(sample.ImagePath).Name;

                DrawBoundingBox(imagesFolder, outputDirectory, filename, filteredBoxes);

                Console.WriteLine(".....The objects in the image {0} are detected as below....", sample.Label);
                foreach (var box in filteredBoxes)
                {
                    Console.WriteLine(box.Label + " and its Confidence score: " + box.Confidence);
                }
                Console.WriteLine("");
            }
        }

        private static IEnumerable<ImageNetData> GetImagesData(string folder)
        {
            List<ImageNetData> imagesList = new List<ImageNetData>();
            string[] filePaths = Directory.GetFiles(folder).Where(filePath => Path.GetExtension(filePath) != ".md").ToArray();
            foreach (var filePath in filePaths)
            {
                ImageNetData imagedata = new ImageNetData { ImagePath = filePath, Label = Path.GetFileName(filePath) };
                imagesList.Add(imagedata);
            }
            return imagesList;
        }
      
        private IDataView CreateEmptyDataView()
        {
            //Create empty DataView. We just need the schema to call fit()
            List<ImageNetData> list = new List<ImageNetData>();
            IEnumerable<ImageNetData> enumerableData = list;
            var dv = mlContext.Data.LoadFromEnumerable(enumerableData);
            return dv;
        }

        public void DrawBoundingBox(string inputImageLocation, string outputImageLocation, string imageName, IList<YoloBoundingBox> filteredBoundingBoxes)
        {
            Image image = Image.FromFile(Path.Combine(inputImageLocation, imageName));

            var originalImageHeight = image.Height;
            var originalImageWidth = image.Width;

            foreach (var box in filteredBoundingBoxes)
            {
                // Get Bounding Box Dimensions
                var x = (uint)Math.Max(box.Dimensions.X, 0);
                var y = (uint)Math.Max(box.Dimensions.Y, 0);
                var width = (uint)Math.Min(originalImageWidth - x, box.Dimensions.Width);
                var height = (uint)Math.Min(originalImageHeight - y, box.Dimensions.Height);

                // Resize To Image
                x = (uint)originalImageWidth * x / 416;
                y = (uint)originalImageHeight * y / 416;
                width = (uint)originalImageWidth * width / 416;
                height = (uint)originalImageHeight * height / 416;

                // Bounding Box Text
                string text = $"{box.Label} ({(box.Confidence * 100).ToString("0")}%)";

                using (Graphics thumbnailGraphic = Graphics.FromImage(image))
                {
                    thumbnailGraphic.CompositingQuality = CompositingQuality.HighQuality;
                    thumbnailGraphic.SmoothingMode = SmoothingMode.HighQuality;
                    thumbnailGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    // Define Text Options
                    Font drawFont = new Font("Arial", 12, FontStyle.Bold);
                    SizeF size = thumbnailGraphic.MeasureString(text, drawFont);
                    SolidBrush fontBrush = new SolidBrush(Color.Black);
                    Point atPoint = new Point((int)x, (int)y - (int)size.Height - 1);

                    // Define BoundingBox options
                    Pen pen = new Pen(box.BoxColor, 3.2f);
                    SolidBrush colorBrush = new SolidBrush(box.BoxColor);

                    // Draw text on image 
                    thumbnailGraphic.FillRectangle(colorBrush, (int)x, (int)(y - size.Height - 1), (int)size.Width, (int)size.Height);
                    thumbnailGraphic.DrawString(text, drawFont, fontBrush, atPoint);

                    // Draw bounding box on image
                    thumbnailGraphic.DrawRectangle(pen, x, y, width, height);
                }
            }

            if (!Directory.Exists(outputImageLocation))
            {
                Directory.CreateDirectory(outputImageLocation);
            }

            image.Save(Path.Combine(outputImageLocation, imageName));
        }
    }
}

