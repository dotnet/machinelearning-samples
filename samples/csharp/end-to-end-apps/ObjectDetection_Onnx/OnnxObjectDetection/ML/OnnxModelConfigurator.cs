using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using System.Collections.Generic;
using System.Linq;

namespace OnnxObjectDetection
{
    public class OnnxModelConfigurator
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _mlModel;

        public OnnxModelConfigurator(string onnxModelFilePath)
        {
            _mlContext = new MLContext();
            // Model creation and pipeline definition for images needs to run just once, so calling it from the constructor:
            _mlModel = SetupMlNetModel(onnxModelFilePath);
        }

        public struct ImageSettings
        {
            public const int imageHeight = 416;
            public const int imageWidth = 416;
        }

        public struct TinyYoloModelSettings
        {
            // for checking TIny yolo2 Model input and  output  parameter names,
            // you can use tools like Netron, 
            // which is installed by Visual Studio AI Tools

            // input tensor name
            public const string ModelInput = "image";

            // output tensor name
            public const string ModelOutput = "grid";
        }

        public ITransformer SetupMlNetModel(string onnxModelFilePath)
        {
            var dataView = _mlContext.Data.LoadFromEnumerable(new List<ImageInputData>());

            var pipeline = _mlContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "image", imageWidth: ImageSettings.imageWidth, imageHeight: ImageSettings.imageHeight, inputColumnName: nameof(ImageInputData.Image))
                            .Append(_mlContext.Transforms.ExtractPixels(outputColumnName: "image"))
                            .Append(_mlContext.Transforms.ApplyOnnxModel(modelFile: onnxModelFilePath, outputColumnNames: new[] { TinyYoloModelSettings.ModelOutput }, inputColumnNames: new[] { TinyYoloModelSettings.ModelInput }));

            var mlNetModel = pipeline.Fit(dataView);

            return mlNetModel;
        }

        public void SaveMLNetModel(string mlnetModelFilePath)
        {
            // Save/persist the model to a .ZIP file to be loaded by the PredictionEnginePool
            _mlContext.Model.Save(_mlModel, null, mlnetModelFilePath);
        }
    }
}

