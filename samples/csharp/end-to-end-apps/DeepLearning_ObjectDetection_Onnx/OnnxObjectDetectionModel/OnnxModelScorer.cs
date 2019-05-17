using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Transforms.Image;

namespace OnnxObjectDetectionModel
{
    public interface IOnnxModelScorer
    {
        void CreateSaveModel();
    }

    public class OnnxModelScorer : IOnnxModelScorer
    {
        private readonly string imagesFolderPath;
        private readonly string inputModelPath;
        private readonly string outputModelPath;
        private readonly MLContext _mlContext;

        public OnnxModelScorer(MLContext mlContext,string imagesFolderPath,  string inputModelPath, string outputModelPath)
        {
            this.imagesFolderPath = imagesFolderPath;
            this.inputModelPath = inputModelPath;
            this.outputModelPath = outputModelPath;
            _mlContext = mlContext;
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

        public void CreateSaveModel()
        {
            Console.WriteLine("Read model");
            Console.WriteLine($"input Model location: {inputModelPath}");
            Console.WriteLine($"Images folder: {imagesFolderPath}");
            Console.WriteLine($"output Model path: {outputModelPath}");
            Console.WriteLine($"Default parameters: image size=({ImageNetSettings.imageWidth},{ImageNetSettings.imageHeight})");

            var dataView = CreateDataView();

            var pipeline = _mlContext.Transforms.LoadImages(outputColumnName: "image", imageFolder: imagesFolderPath, inputColumnName: nameof(ImageNetData.ImagePath))
                            .Append(_mlContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "image", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: "image"))
                            .Append(_mlContext.Transforms.ExtractPixels(outputColumnName: "image"))
                            .Append(_mlContext.Transforms.ApplyOnnxModel(modelFile: inputModelPath, outputColumnNames: new[] { TinyYoloModelSettings.ModelOutput }, inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));

            var model = pipeline.Fit(dataView);

            // STEP 6: Save/persist the trained model to a .ZIP file
            Console.WriteLine("=============== Saving the model to a file ===============");
            _mlContext.Model.Save(model, dataView.Schema, outputModelPath);
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

