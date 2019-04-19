using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Transforms.Image;

namespace OnnxObjectDetectionWebAPI.OnnxModelScorers
{
    public interface IOnnxModelScorer
    {
        IList<string> DetectObjectsUsingModel(string imagesFilePath);
        PredictionEngine<ImageNetData, ImageNetPrediction> CreatePredictionEngine(string imagesFolder, string modelLocation);
        void PaintImages(string imageFilePath);
    }

    public class OnnxModelScorer : IOnnxModelScorer
    {
        private readonly string _imagesLocation;
        private readonly string _imagesTmpFolder;
        private readonly string _modelLocation;
        private readonly MLContext _mlContext;
        IList<YoloBoundingBox> filteredBoxes;

        private IList<YoloBoundingBox> _boxes = new List<YoloBoundingBox>();
        private readonly YoloWinMlParser _parser = new YoloWinMlParser();

#pragma warning disable IDE0032
        private readonly PredictionEngine<ImageNetData, ImageNetPrediction> _predictionEngine;
        public PredictionEngine<ImageNetData, ImageNetPrediction> PredictionEngine
        {
            get => _predictionEngine;
        }

        public OnnxModelScorer()
        {
            var assetsPath = ModelHelpers.GetFolderFullPath(@"Model");
            _imagesTmpFolder = ModelHelpers.GetFolderFullPath(@"ImagesTemp");
            _modelLocation = Path.Combine(assetsPath, "TinyYolo2_model.onnx");
            
            _mlContext = new MLContext();

            // Create the prediction function in the constructor, once, as it is an expensive operation
            // Note that, on average, this call takes around 200x longer than one prediction, so you want to cache it
            // and reuse the prediction function, instead of creating one per prediction.
            // IMPORTANT: Remember that the 'Predict()' method is not reentrant. 
            // If you want to use multiple threads for simultaneous prediction, 
            // make sure each thread is using its own PredictionFunction (e.g. In DI/IoC use .AddScoped())
            // or use a critical section when using the Predict() method.
            _predictionEngine = this.CreatePredictionEngine(_imagesTmpFolder, _modelLocation);
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

        public PredictionEngine<ImageNetData, ImageNetPrediction> CreatePredictionEngine(string imagesFolder, string modelLocation)
        {
            Console.WriteLine("Read model");
            Console.WriteLine($"Model location: {modelLocation}");
            Console.WriteLine($"Images folder: {imagesFolder}");
            Console.WriteLine($"Default parameters: image size=({ImageNetSettings.imageWidth},{ImageNetSettings.imageHeight})");

            var dataView = CreateDataView();

            var pipeline = _mlContext.Transforms.LoadImages(outputColumnName: "image", imageFolder: imagesFolder, inputColumnName: nameof(ImageNetData.ImagePath))
                            .Append(_mlContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "image", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: "image"))
                            .Append(_mlContext.Transforms.ExtractPixels(outputColumnName: "image"))
                            .Append(_mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { TinyYoloModelSettings.ModelOutput }, inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));

            var model = pipeline.Fit(dataView);

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ImageNetData, ImageNetPrediction>(model);

            return predictionEngine;
        }

        public IList<string> DetectObjectsUsingModel(string imagesFilePath)
        {
            var imageInputData = new ImageNetData { ImagePath = imagesFilePath };
            var probs = _predictionEngine.Predict(imageInputData).PredictedLabels;
            IList<YoloBoundingBox> boundingBoxes = _parser.ParseOutputs(probs);
            filteredBoxes = _parser.NonMaxSuppress(boundingBoxes, 5, .5F);
            List<string> objectsNames = new List<string>();
            foreach (var box in filteredBoxes)
            {
                objectsNames.Add(box.Label);
            }
            return objectsNames;
        }

        public void PaintImages(string imageFilePath)
        {
          Image image = Image.FromFile(imageFilePath);
          var originalHeight = image.Height;
          var originalWidth = image.Width;      
          foreach (var box in filteredBoxes)
          {
              //// process output boxes
              var x = (uint)Math.Max(box.X, 0);
              var y = (uint)Math.Max(box.Y, 0);
              var w = (uint)Math.Min(originalWidth - x, box.Width);
              var h = (uint)Math.Min(originalHeight - y, box.Height);

              // fit to current image size
              x = (uint)originalWidth * x / 416;
              y = (uint)originalHeight * y / 416;
              w = (uint)originalWidth * w / 416;
              h = (uint)originalHeight * h / 416;

              string text = string.Format("{0} ({1})", box.Label, box.Confidence);

              using (Graphics thumbnailGraph = Graphics.FromImage(image))
              {
                  thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                  thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                  thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                  Font drawFont = new Font("Arial", 16);
                  SolidBrush redBrush = new SolidBrush(Color.Red);
                  Point atPoint = new Point((int)x, (int)y);
                  Pen pen = new Pen(Color.Yellow, 4.0f);
                  SolidBrush yellowBrush = new SolidBrush(Color.Yellow);

                  // Fill rectangle on which the text is displayed.
                  RectangleF rect = new RectangleF(x, y, 130, 20);                  
                  thumbnailGraph.FillRectangle(yellowBrush, rect);
                  //draw text in red color
                  thumbnailGraph.DrawString(text, drawFont, redBrush, atPoint);  
                  //draw rectangle around object
                  thumbnailGraph.DrawRectangle(pen, x, y, w, h);
              }
            }
            string outputImage = "Image";            
            var xim = string.Format("{0}_{1}", outputImage, 1);
            image.Save(string.Format(@"output\{0}.jpg",xim));
        }

        private IDataView CreateDataView()
        {
            //Create empty DataView. We just need the schema to call fit()
            List<ImageNetData> list = new List<ImageNetData>();
            //list.Add(new ImageInputData() { ImagePath = "image-name.jpg" });   //Since we just need the schema, no need to provide anything here
            IEnumerable<ImageNetData> enumerableData = list;
            var dv = _mlContext.Data.LoadFromEnumerable(enumerableData);
            return dv;
        }
    }
}

