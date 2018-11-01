using System;
using System.Collections.Generic;
using System.Linq;
using ImageClassification.ImageData;
using static ImageClassification.Model.ConsoleHelpers;
using static ImageClassification.Model.ModelHelpers;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.ImageAnalytics;
using System.IO;
using Microsoft.ML.Transforms;

namespace ImageClassification.Model
{
    public class ModelScorerCustom
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string modelLocation;
        private readonly string labelsLocation;
        private readonly ConsoleEnvironment env;

        public ModelScorerCustom(string dataLocation, string imagesFolder, string modelLocation, string labelsLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.modelLocation = modelLocation;
            this.labelsLocation = labelsLocation;
            env = new ConsoleEnvironment();
        }

        public struct ImageNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const bool channelsLast = true;
        }

        public void Score()
        {
            

            var model = LoadModel(dataLocation, imagesFolder, modelLocation);

            var predictions = PredictDataUsingModel(dataLocation, imagesFolder, labelsLocation, model).ToArray();

            //EvaluateModel(dataLocation, model);
        }

        private PredictionFunction<ImageNetData, ImageNetPrediction> LoadModel(string dataLocation, string imagesFolder, string modelLocation)
        {
            ConsoleWriteHeader("Read model");
            Console.WriteLine($"Model location: {modelLocation}");

            var loader = TextLoader.CreateReader(env, 
                ctx => (ImagePath: ctx.LoadText(0), Label: ctx.LoadText(1)) );

            var data = loader.Read(new MultiFileSource(dataLocation));

            var estimator = loader.MakeNewEstimator()
                .Append(row => (
                    row.Label,
                    input_1: row.ImagePath
                                .LoadAsImage(imagesFolder)
                                .Resize(ImageNetSettings.imageHeight, ImageNetSettings.imageWidth)
                                .ExtractPixels(interleaveArgb: ImageNetSettings.channelsLast, offset: ImageNetSettings.mean)))
                .Append(row => (row.Label, dense_2_Softmax : row.input_1.ApplyTensorFlowGraph(modelLocation)));

            var model = estimator.Fit(data);

            var predictionFunction = model.AsDynamic.MakePredictionFunction<ImageNetData, ImageNetPrediction>(env);

            return predictionFunction;
        }

        protected IEnumerable<ImageNetData> PredictDataUsingModel(string testLocation, string imagesFolder, string labelsLocation, PredictionFunction<ImageNetData, ImageNetPrediction> model)
        {
            ConsoleWriteHeader("Classificate images");
            Console.WriteLine($"Images folder: {imagesFolder}");
            Console.WriteLine($"Training file: {testLocation}");
            Console.WriteLine($"Labels file: {labelsLocation}");

            var labels = ModelHelpers.ReadLabels(labelsLocation);

            //var testData = ImageNetData.ReadFromCsv(testLocation, imagesFolder);
            var testData = new[] {
                new ImageNetData { ImagePath = Path.Combine(imagesFolder, "bracelet.jpg"), Label = "bracelet" },
                new ImageNetData { ImagePath = Path.Combine(imagesFolder, "frisbee.jpg"), Label = "frisbee" }
            };

            foreach (var sample in testData)
            {
                var probs = model.Predict(sample).PredictedLabels;
                var imageData = new ImageNetDataProbability()
                {
                    ImagePath = sample.ImagePath,
                };
                (imageData.Label, imageData.Probability) = GetLabel(labels, probs);
                imageData.ConsoleWrite();
                yield return imageData;
            }
        }

        protected void EvaluateModel(string testLocation, PredictionFunction<ImageNetData, ImageNetPrediction> model)
        {
            ConsoleWriteHeader("Metrics for Image Classification");
            var evaluator = new MultiClassClassifierEvaluator(env, new MultiClassClassifierEvaluator.Arguments() );

            var data = TextLoader.ReadFile(env,
                new TextLoader.Arguments
                {
                    Separator = "tab",
                    HasHeader = false,
                    Column = new []
                    {
                        new TextLoader.Column("ImagePath", DataKind.Text, 0),
                        new TextLoader.Column("Label", DataKind.Text, 1)
                    }
                },
                new MultiFileSource(testLocation));

            var evalRoles = new RoleMappedData(data, label: "Label", feature: "ImagePath");

            var metrics = evaluator.Evaluate(evalRoles);

            foreach (var item in metrics)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
        }
    }
}
