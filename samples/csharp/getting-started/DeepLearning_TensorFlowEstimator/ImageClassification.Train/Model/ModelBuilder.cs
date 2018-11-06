using System;
using System.IO;
using System.Linq;
using ImageClassification.ImageData;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.ImageAnalytics;
using Microsoft.ML.Transforms;
using Microsoft.ML.Runtime;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Runtime.Api;
using static ImageClassification.Model.ConsoleHelpers;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Transforms.Categorical;
using Microsoft.ML.Transforms.Conversions;

namespace ImageClassification.Model
{
    public class ModelBuilder
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string inputModelLocation;
        private readonly string outputModelLocation;
        private readonly IHostEnvironment env;

        public ModelBuilder(string dataLocation, string imagesFolder, string inputModelLocation, string outputModelLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.inputModelLocation = inputModelLocation;
            this.outputModelLocation = outputModelLocation;
            env = new ConsoleEnvironment(seed: 1);
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



            var loader = new TextLoader(env,
                new TextLoader.Arguments
                {
                    Column = new[] {
                        new TextLoader.Column("ImagePath", DataKind.Text, 0),
                        new TextLoader.Column("Label", DataKind.Text, 1)
                    }
                });



            var pipeline = new ValueToKeyMappingEstimator(env, "Label", "LabelTokey")
                        .Append(new ImageLoadingEstimator(env, imagesFolder, ("ImagePath", "ImageReal")))
                        .Append(new ImageResizingEstimator(env, "ImageReal", "ImageReal", ImageNetSettings.imageHeight, ImageNetSettings.imageWidth))
                        .Append(new ImagePixelExtractingEstimator(env, new[] { new ImagePixelExtractorTransform.ColumnInfo("ImageReal", "input", interleave: ImageNetSettings.channelsLast, offset: ImageNetSettings.mean) }))
                        .Append(new TensorFlowEstimator(env, featurizerModelLocation, new[] { "input" }, new[] { "softmax2_pre_activation" }))
                        .Append(new SdcaMultiClassTrainer(env, "softmax2_pre_activation", "LabelTokey"))
                        .Append(new KeyToValueEstimator(env, ("PredictedLabel", "PredictedLabelValue")));

            // Train the pipeline
            ConsoleWriteHeader("Training classification model");
            var data = loader.Read(new MultiFileSource(dataLocation));
            var model = pipeline.Fit(data);

            // Process the training data through the model
            // This is an optional step, but it's useful for debugging issues
            var trainData = model.Transform(data);
            var loadedModelOutputColumnNames = trainData.Schema.GetColumnNames();
            var trainData2 = trainData.AsEnumerable<ImageNetPipeline>(env, false, true).ToList();
            trainData2.ForEach(pr => ConsoleWriteImagePrediction(pr.ImagePath,pr.PredictedLabelValue, pr.Score.Max()));

            // Get some performance metric on the model using training data            
            var sdcaContext = new MulticlassClassificationContext(env);
            ConsoleWriteHeader("Classification metrics");
            var metrics = sdcaContext.Evaluate(trainData, label: "LabelTokey", predictedLabel: "PredictedLabel");
            Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");

            // Save the model to assets/outputs
            ConsoleWriteHeader("Save model to local file");
            ModelHelpers.DeleteAssets(outputModelLocation);
            using (var f = new FileStream(outputModelLocation, FileMode.Create))
                model.SaveTo(env, f);
            Console.WriteLine($"Model saved: {outputModelLocation}");
        }

    }
}
