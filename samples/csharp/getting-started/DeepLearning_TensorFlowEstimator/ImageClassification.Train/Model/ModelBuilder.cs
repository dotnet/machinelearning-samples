using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.ImageAnalytics;
using ImageClassification.ImageData;
using static ImageClassification.Model.ConsoleHelpers;
using Microsoft.ML.Core.Data;

namespace ImageClassification.Model
{
    public class ModelBuilder
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string inputModelLocation;
        private readonly string outputModelLocation;
        private readonly MLContext mlContext;
        private static string LabelTokey = nameof(LabelTokey);
        private static string ImageReal = nameof(ImageReal);
        private static string PredictedLabelValue = nameof(PredictedLabelValue);

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



            var data = mlContext.Data.ReadFromTextFile<ImageNetData>(path:dataLocation, hasHeader: false);

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: LabelTokey,inputColumnName:DefaultColumnNames.Label)
                            .Append(mlContext.Transforms.LoadImages(imagesFolder, (ImageReal, nameof(ImageNetData.ImagePath))))
                            .Append(mlContext.Transforms.Resize(outputColumnName:ImageReal, imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: ImageReal))
                            .Append(mlContext.Transforms.ExtractPixels(new ImagePixelExtractorTransformer.ColumnInfo(name: "input",inputColumnName: ImageReal,  interleave: ImageNetSettings.channelsLast, offset: ImageNetSettings.mean)))
                            .Append(mlContext.Transforms.ScoreTensorFlowModel(modelLocation:featurizerModelLocation, outputColumnNames: new[] { "softmax2_pre_activation" },inputColumnNames: new[] { "input" }))
                            .Append(mlContext.MulticlassClassification.Trainers.LogisticRegression(labelColumn:LabelTokey, featureColumn:"softmax2_pre_activation"))
                            .Append(mlContext.Transforms.Conversion.MapKeyToValue((PredictedLabelValue,DefaultColumnNames.PredictedLabel)));

            // Train the model
            ConsoleWriteHeader("Training classification model");
            ITransformer model = pipeline.Fit(data);

            // Process the training data through the model
            // This is an optional step, but it's useful for debugging issues
            var trainData = model.Transform(data);
            var loadedModelOutputColumnNames = trainData.Schema
                .Where(col => !col.IsHidden).Select(col => col.Name);
            var trainData2 = mlContext.CreateEnumerable<ImageNetPipeline>(trainData, false, true).ToList();
            trainData2.ForEach(pr => ConsoleWriteImagePrediction(pr.ImagePath,pr.PredictedLabelValue, pr.Score.Max()));

            // Get some performance metric on the model using training data            
            var classificationContext = new MulticlassClassificationCatalog(mlContext);
            ConsoleWriteHeader("Classification metrics");
            var metrics = classificationContext.Evaluate(trainData, label: LabelTokey, predictedLabel: DefaultColumnNames.PredictedLabel);
            Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");

            // Save the model to assets/outputs
            ConsoleWriteHeader("Save model to local file");
            ModelHelpers.DeleteAssets(outputModelLocation);
            using (var f = new FileStream(outputModelLocation, FileMode.Create))
                mlContext.Model.Save(model, f);

            Console.WriteLine($"Model saved: {outputModelLocation}");
        }

    }
}
