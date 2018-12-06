using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.ImageAnalytics;
using Microsoft.ML.Transforms;
using Microsoft.ML;

using ImageClassification.ImageDataStructures;
using static ImageClassification.ModelScorer.ConsoleHelpers;
using static ImageClassification.ModelScorer.ModelHelpers;

namespace ImageClassification.ModelScorer
{
    public class TFModelScorer
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string modelLocation;
        private readonly string labelsLocation;
        private readonly MLContext mlContext;

        public TFModelScorer(string dataLocation, string imagesFolder, string modelLocation, string labelsLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.modelLocation = modelLocation;
            this.labelsLocation = labelsLocation;
            mlContext = new MLContext();
        }

        public struct ImageNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const bool channelsLast = true;
        }

        public struct InceptionSettings
        {
            // for checking tensor names, you can use tools like Netron,
            // which is installed by Visual Studio AI Tools

            // input tensor name
            public const string inputTensorName = "input";

            // output tensor name
            public const string outputTensorName = "softmax2";
        }

        public void Score()
        {
            var model = LoadModel(dataLocation, imagesFolder, modelLocation);

            var predictions = PredictDataUsingModel(dataLocation, imagesFolder, labelsLocation, model).ToArray();

        }

        private PredictionFunction<ImageNetData, ImageNetPrediction> LoadModel(string dataLocation, string imagesFolder, string modelLocation)
        {
            ConsoleWriteHeader("Read model");
            Console.WriteLine($"Model location: {modelLocation}");
            Console.WriteLine($"Images folder: {imagesFolder}");
            Console.WriteLine($"Training file: {dataLocation}");
            Console.WriteLine($"Default parameters: image size=({ImageNetSettings.imageWidth},{ImageNetSettings.imageHeight}), image mean: {ImageNetSettings.mean}");

            var loader = new TextLoader(mlContext,
                new TextLoader.Arguments
                {
                    Column = new[] {
                        new TextLoader.Column("ImagePath", DataKind.Text, 0),
                    }
                });

            var data = loader.Read(new MultiFileSource(dataLocation));

            var pipeline = mlContext.Transforms.LoadImages(imageFolder: imagesFolder, columns: ("ImagePath", "ImageReal"))
                            .Append(mlContext.Transforms.Resize("ImageReal", "ImageReal", ImageNetSettings.imageHeight, ImageNetSettings.imageWidth))
                            .Append(mlContext.Transforms.ExtractPixels(new[] { new ImagePixelExtractorTransform.ColumnInfo("ImageReal", "input", interleave: ImageNetSettings.channelsLast, offset: ImageNetSettings.mean) }))
                            .Append(mlContext.Transforms.ScoreTensorFlowModel(modelLocation, new[] { "input" }, new[] { "softmax2" }));

            var modeld = pipeline.Fit(data);

            var predictionFunction = modeld.MakePredictionFunction<ImageNetData, ImageNetPrediction>(mlContext);

            return predictionFunction;
        }

        protected IEnumerable<ImageNetData> PredictDataUsingModel(string testLocation, string imagesFolder, string labelsLocation, PredictionFunction<ImageNetData, ImageNetPrediction> model)
        {
            ConsoleWriteHeader("Classificate images");
            Console.WriteLine($"Images folder: {imagesFolder}");
            Console.WriteLine($"Training file: {testLocation}");
            Console.WriteLine($"Labels file: {labelsLocation}");

            var labels = ModelHelpers.ReadLabels(labelsLocation);

            var testData = ImageNetData.ReadFromCsv(testLocation, imagesFolder);

            foreach (var sample in testData)
            {
                var probs = model.Predict(sample).PredictedLabels;
                var imageData = new ImageNetDataProbability()
                {
                    ImagePath = sample.ImagePath,
                    Label = sample.Label
                };
                (imageData.PredictedLabel, imageData.Probability) = GetBestLabel(labels, probs);
                imageData.ConsoleWrite();
                yield return imageData;
            }
        }
    }
}
