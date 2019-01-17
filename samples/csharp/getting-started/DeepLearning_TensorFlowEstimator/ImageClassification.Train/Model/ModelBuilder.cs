using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.ImageAnalytics;
using ImageClassification.ImageData;
using static ImageClassification.Model.ConsoleHelpers;

namespace ImageClassification.Model
{
    public class ModelBuilder
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string inputModelLocation;
        private readonly string outputModelLocation;
        private readonly MLContext mlContext;

        public ModelBuilder(string dataLocation, string imagesFolder, string inputModelLocation, string outputModelLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.inputModelLocation = inputModelLocation;
            this.outputModelLocation = outputModelLocation;
            mlContext = new MLContext(seed: 1);
        }

        private struct ImageNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const float scale = 1;
            public const bool channelsLast = true;
        }

        public void BuildAndTrain()
        {
            var featurizerModelLocation = inputModelLocation;

            ConsoleWriteHeader("Read model");
            Console.WriteLine($"Model location: {featurizerModelLocation}");
            Console.WriteLine($"Images folder: {imagesFolder}");
            Console.WriteLine($"Training file: {dataLocation}");
            Console.WriteLine($"Default parameters: image size=({ImageNetSettings.imageWidth},{ImageNetSettings.imageHeight}), image mean: {ImageNetSettings.mean}");



            var loader = mlContext.Data.CreateTextReader(
                new TextLoader.Arguments
                {
                    Column = new[] {
                        new TextLoader.Column("ImagePath", DataKind.Text, 0),
                        new TextLoader.Column("Label", DataKind.Text, 1)
                    }
                });

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", "LabelTokey")
                            .Append(mlContext.Transforms.LoadImages(imagesFolder, ("ImagePath", "ImageReal")))
                            .Append(mlContext.Transforms.Resize("ImageReal", "ImageReal", ImageNetSettings.imageHeight, ImageNetSettings.imageWidth))
                            .Append(mlContext.Transforms.ExtractPixels(new ImagePixelExtractorTransform.ColumnInfo("ImageReal", "input", interleave: ImageNetSettings.channelsLast, offset: ImageNetSettings.mean)))
                            .Append(mlContext.Transforms.ScoreTensorFlowModel(featurizerModelLocation, new[] { "input" }, new[] { "softmax2_pre_activation" }))
                            .Append(mlContext.MulticlassClassification.Trainers.LogisticRegression("LabelTokey", "softmax2_pre_activation"))
                            .Append(mlContext.Transforms.Conversion.MapKeyToValue(("PredictedLabel", "PredictedLabelValue")));

            // Train the pipeline
            ConsoleWriteHeader("Training classification model");
            var data = loader.Read(dataLocation);
            var model = pipeline.Fit(data);

            // Process the training data through the model
            // This is an optional step, but it's useful for debugging issues
            var trainData = model.Transform(data);
            var loadedModelOutputColumnNames = trainData.Schema
                .Where(col => !col.IsHidden).Select(col => col.Name);
            var trainData2 = trainData.AsEnumerable<ImageNetPipeline>(mlContext, false, true).ToList();
            trainData2.ForEach(pr => ConsoleWriteImagePrediction(pr.ImagePath,pr.PredictedLabelValue, pr.Score.Max()));

            // Get some performance metric on the model using training data            
            var sdcaContext = new MulticlassClassificationContext(mlContext);
            ConsoleWriteHeader("Classification metrics");
            var metrics = sdcaContext.Evaluate(trainData, label: "LabelTokey", predictedLabel: "PredictedLabel");
            Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");

            // Save the model to assets/outputs
            ConsoleWriteHeader("Save model to local file");
            ModelHelpers.DeleteAssets(outputModelLocation);
            using (var f = new FileStream(outputModelLocation, FileMode.Create))
                model.SaveTo(mlContext, f);
            Console.WriteLine($"Model saved: {outputModelLocation}");
        }

    }
}
