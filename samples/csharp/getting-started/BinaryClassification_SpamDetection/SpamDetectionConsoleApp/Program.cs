using System;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Microsoft.ML;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime;
using Microsoft.ML.Core.Data;
using SpamDetectionConsoleApp.MLDataStructures;

namespace SpamDetectionConsoleApp
{
    class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string DataDirectoryPath => Path.Combine(AppPath, "..", "..", "..", "Data", "spamfolder");
        private static string TrainDataPath => Path.Combine(AppPath, "..", "..", "..", "Data", "spamfolder", "SMSSpamCollection");

        static void Main(string[] args)
        {
            // Download the dataset if it doesn't exist.
            if (!File.Exists(TrainDataPath))
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile("https://archive.ics.uci.edu/ml/machine-learning-databases/00228/smsspamcollection.zip", "spam.zip");
                }

                ZipFile.ExtractToDirectory("spam.zip", DataDirectoryPath);
            }
            
            // Set up the MLContext, which is a catalog of components in ML.NET.
            var mlContext = new MLContext();

            // Create the reader and define which columns from the file should be read.
            var reader = new TextLoader(mlContext, new TextLoader.Arguments()
            {
                Separator = "tab",
                HasHeader = true,
                Column = new[]
                    {
                        new TextLoader.Column("Label", DataKind.Text, 0),
                        new TextLoader.Column("Message", DataKind.Text, 1)
                    }
            });

            var data = reader.Read(new MultiFileSource(TrainDataPath));

            // Create the estimator which converts the text label to boolean, featurizes the text, and adds a linear trainer.
            var estimator = mlContext.Transforms.CustomMapping<MyInput, MyOutput>(MyLambda.MyAction, "MyLambda")
                .Append(mlContext.Transforms.Text.FeaturizeText("Message", "Features"))
                .Append(mlContext.BinaryClassification.Trainers.StochasticDualCoordinateAscent());

            // Evaluate the model using cross-validation.
            // Cross-validation splits our dataset into 'folds', trains a model on some folds and 
            // evaluates it on the remaining fold. We are using 5 folds so we get back 5 sets of scores.
            // Let's compute the average AUC, which should be between 0.5 and 1 (higher is better).
            var cvResults = mlContext.BinaryClassification.CrossValidate(data, estimator, numFolds: 5);
            var aucs = cvResults.Select(r => r.metrics.Auc);
            Console.WriteLine("The AUC is {0}", aucs.Average());

            // Now let's train a model on the full dataset to help us get better results
            var model = estimator.Fit(data);

            // The dataset we have is skewed, as there are many more non-spam messages than spam messages.
            // While our model is relatively good at detecting the difference, this skewness leads it to always
            // say the message is not spam. We deal with this by lowering the threshold of the predictor. In reality,
            // it is useful to look at the precision-recall curve to identify the best possible threshold.
            var inPipe = new TransformerChain<ITransformer>(model.Take(model.Count() - 1).ToArray());
            var lastTransformer = new BinaryPredictionTransformer<IPredictorProducing<float>>(mlContext, model.LastTransformer.Model, inPipe.GetOutputSchema(data.Schema), model.LastTransformer.FeatureColumn, threshold: 0.15f, thresholdColumn: DefaultColumnNames.Probability);

            ITransformer[] parts = model.ToArray();
            parts[parts.Length - 1] = lastTransformer;
            var newModel = new TransformerChain<ITransformer>(parts);

            // Create a PredictionFunction from our model 
            var predictor = newModel.MakePredictionFunction<SpamInput, SpamPrediction>(mlContext);

            // Test a few examples
            ClassifyMessage(predictor, "That's a great idea. It should work.");
            ClassifyMessage(predictor, "free medicine winner! congratulations");
            ClassifyMessage(predictor, "Yes we should meet over the weekend!");
            ClassifyMessage(predictor, "you win pills and free entry vouchers");

            Console.ReadLine();
        }

        public class MyInput
        {
            public string Label { get; set; }
        }

        public class MyOutput
        {
            public bool Label { get; set; }
        }

        public class MyLambda
        {
            [Export("MyLambda")]
            public ITransformer MyTransformer => ML.Transforms.CustomMappingTransformer<MyInput, MyOutput>(MyAction, "MyLambda");

            [Import]
            public MLContext ML { get; set; }

            public static void MyAction(MyInput input, MyOutput output)
            {
                output.Label = input.Label == "spam" ? true : false;
            }
        }

        public static void ClassifyMessage(PredictionFunction<SpamInput, SpamPrediction> predictor, string message)
        {
            var input = new SpamInput { Message = message };
            var prediction = predictor.Predict(input);

            Console.WriteLine("The message '{0}' is {1}", input.Message, prediction.isSpam ? "spam" : "not spam");
        }
    }
}
