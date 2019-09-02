using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using System.Collections.Generic;
using System.Linq;

namespace OnnxObjectDetection
{
    public class OnnxModelConfigurator
    {
        private readonly MLContext mlContext;
        private readonly ITransformer mlModel;

        public OnnxModelConfigurator(IOnnxModel onnxModel)
        {
            mlContext = new MLContext();
            // Model creation and pipeline definition for images needs to run just once,
            // so calling it from the constructor:
            mlModel = SetupMlNetModel(onnxModel);
        }

        private ITransformer SetupMlNetModel(IOnnxModel onnxModel)
        {
            var dataView = mlContext.Data.LoadFromEnumerable(new List<ImageInputData>());

            var pipeline = mlContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: onnxModel.ModelInput, imageWidth: ImageSettings.imageWidth, imageHeight: ImageSettings.imageHeight, inputColumnName: nameof(ImageInputData.Image))
                            .Append(mlContext.Transforms.ExtractPixels(outputColumnName: onnxModel.ModelInput))
                            .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: onnxModel.ModelPath, outputColumnName: onnxModel.ModelOutput, inputColumnName: onnxModel.ModelInput));

            var mlNetModel = pipeline.Fit(dataView);

            return mlNetModel;
        }

        public PredictionEngine<ImageInputData, T> GetMlNetPredictionEngine<T>()
            where T : class, IOnnxObjectPrediction, new()
        {
            return mlContext.Model.CreatePredictionEngine<ImageInputData, T>(mlModel);
        }

        public void SaveMLNetModel(string mlnetModelFilePath)
        {
            // Save/persist the model to a .ZIP file to be loaded by the PredictionEnginePool
            mlContext.Model.Save(mlModel, null, mlnetModelFilePath);
        }
    }
}
