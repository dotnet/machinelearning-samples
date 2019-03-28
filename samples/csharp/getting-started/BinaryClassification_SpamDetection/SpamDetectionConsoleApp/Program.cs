using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
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
                    //The code below will download a dataset from a third-party, UCI (link), and may be governed by separate third-party terms.
                    //By proceeding, you agree to those separate terms.
                    client.DownloadFile("https://archive.ics.uci.edu/ml/machine-learning-databases/00228/smsspamcollection.zip", "spam.zip");
                }

                ZipFile.ExtractToDirectory("spam.zip", DataDirectoryPath);
            }

            // Set up the MLContext, which is a catalog of components in ML.NET.
            MLContext mlContext = new MLContext();

            // Specify the schema for spam data and read it into DataView.
            var data = mlContext.Data.LoadFromTextFile<SpamInput>(path: TrainDataPath, hasHeader: true, separatorChar: '\t');

            // Create the estimator which converts the text label to boolean, featurizes the text, and adds a linear trainer.
            var dataProcessPipeLine = mlContext.Transforms.CustomMapping<InputRow, OutputRow>(CustomMappings.IncomeMapping, nameof(CustomMappings.IncomeMapping))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: DefaultColumnNames.Features, inputColumnName: nameof(SpamInput.Message)))
                                // The dataset we have is skewed, as there are many more non-spam messages than spam messages.
                                // While our model is relatively good at detecting the difference, this skewness leads it to always
                                // say the message is not spam. We deal with this by lowering the threshold of the predictor. In reality,
                                // it is useful to look at the precision-recall curve to identify the best possible threshold.
                .Append(mlContext.Transforms.Normalize(new NormalizingEstimator.LogMeanVarColumnOptions("LogMeanVarNormalized", DefaultColumnNames.Features, useCdf: true)))
                .AppendCacheCheckpoint(mlContext);

            //Create the training pipeline
            Console.WriteLine("=============== Training the model ===============");
            var trainingPipeLine = dataProcessPipeLine.Append(mlContext.BinaryClassification.Trainers.StochasticDualCoordinateAscent(featureColumnName: "LogMeanVarNormalized"));

            // Evaluate the model using cross-validation.
            // Cross-validation splits our dataset into 'folds', trains a model on some folds and
            // evaluates it on the remaining fold. We are using 5 folds so we get back 5 sets of scores.
            // Let's compute the average AUC, which should be between 0.5 and 1 (higher is better).
            Console.WriteLine("=============== Cross-validating to get model's accuracy metrics ===============");
            var crossValidationResults = mlContext.BinaryClassification.CrossValidate(data:data, estimator:trainingPipeLine, numFolds: 5);
            var aucs = crossValidationResults.Select(r => r.Metrics.Auc);
            Console.WriteLine("The AUC is {0}", aucs.Average());

            // Now let's train a model on the full dataset to help us get better results
            var model = trainingPipeLine.Fit(data);

            // Create a PredictionFunction from our model
            var predictor = model.CreatePredictionEngine<SpamInput, SpamPrediction>(mlContext);

            Console.WriteLine("=============== Predictions for below data===============");
            // Test a few examples
            ClassifyMessage(predictor, "That's a great idea. It should work.");
            ClassifyMessage(predictor, "free medicine winner! congratulations");
            ClassifyMessage(predictor, "Yes we should meet over the weekend!");
            ClassifyMessage(predictor, "you win pills and free entry vouchers");

            Console.WriteLine("=============== End of process, hit any key to finish =============== ");
            Console.ReadLine();
        }

        public static void ClassifyMessage(PredictionEngine<SpamInput, SpamPrediction> predictor, string message)
        {
            var input = new SpamInput { Message = message };
            var prediction = predictor.Predict(input);

            Console.WriteLine("The message '{0}' is {1}", input.Message, prediction.isSpam ? "spam" : "not spam");
        }
    }
}
