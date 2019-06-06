using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using OnnxObjectDetectionE2EAPP.Utilities;

namespace OnnxObjectDetectionE2EAPP.MLModel
{
    public static class OnnxModelSettings 
    {
        private static MLContext _mlContext = new MLContext();

        private static string assetsRelativePath = @"Assets";

        private static string inputModelRelativePath = Path.Combine(assetsRelativePath, "inputs", "model", "TinyYolo2_model.onnx");
        private static string outputModelRelativePath = Path.Combine(assetsRelativePath, "outputs", "TinyYoloModel.zip");
       
        private static string inputModelPath = CommonHelpers.GetAbsolutePath(inputModelRelativePath);
        private static string outputModelPath = CommonHelpers.GetAbsolutePath(outputModelRelativePath);      

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

        public static void SetupModel()
        {
            var dataView = CreateEmptyDataView();

            var pipeline = _mlContext.Transforms.LoadImages(outputColumnName: "image", imageFolder: "", inputColumnName: nameof(ImageNetData.ImagePath))
                            .Append(_mlContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "image", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: "image"))
                            .Append(_mlContext.Transforms.ExtractPixels(outputColumnName: "image"))
                            .Append(_mlContext.Transforms.ApplyOnnxModel(modelFile: inputModelPath, outputColumnNames: new[] { TinyYoloModelSettings.ModelOutput }, inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));

            var model = pipeline.Fit(dataView);

            // STEP 6: Save/persist the trained model to a .ZIP file
            _mlContext.Model.Save(model, dataView.Schema, outputModelPath);
        }
       
        private static IDataView CreateEmptyDataView()
        {
            //Create empty DataView. We just need the schema to call fit()
            List<ImageNetData> list = new List<ImageNetData>();
            IEnumerable<ImageNetData> enumerableData = list;
            var dv = _mlContext.Data.LoadFromEnumerable(enumerableData);
            return dv;
        }
    }
}

