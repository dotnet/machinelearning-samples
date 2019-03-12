using System;
using System.IO;
using System.Net;
using Microsoft.ML;
using Microsoft.ML.Data;

using SentimentAnalysisConsoleApp.DataStructures;
using Common;
using Microsoft.Data.DataView;

namespace SentimentAnalysisConsoleApp
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static readonly string BaseDatasetsRelativePath = @"../../../../Data";
        private static readonly string TrainDataRelativePath = $"{BaseDatasetsRelativePath}/wikiDetoxAnnotated160kRows.tsv";

        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);

        private static readonly string BaseModelsRelativePath = @"../../../../MLModels";
        private static readonly string ModelRelativePath = $"{BaseModelsRelativePath}/SentimentModel.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        static void Main(string[] args)
        {
            //Create MLContext to be shared across the model creation workflow objects 
            //Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 1);

            DownloadDataIfNotExists(GetAbsolutePath(BaseDatasetsRelativePath));

            // Create, Train, Evaluate and Save a model
            BuildTrainEvaluateAndSaveModel(mlContext);
            Common.ConsoleHelper.ConsoleWriteHeader("=============== End of training process ===============");

            // Make a single test prediction loding the model from .ZIP file
            TestSinglePrediction(mlContext);

            Common.ConsoleHelper.ConsoleWriteHeader("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();

        }

        private static ITransformer BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // STEP 1: Common data loading configuration
            IDataView dataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(TrainDataPath, hasHeader: true);

            var testTrainSplit = mlContext.BinaryClassification.TrainTestSplit(dataView, testFraction: 0.2);

            // STEP 2: Common data process configuration with pipeline data transformations          
            var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: DefaultColumnNames.Features, inputColumnName:nameof(SentimentIssue.Text));

            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            ConsoleHelper.PeekDataViewInConsole(mlContext, dataView, dataProcessPipeline, 2);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, DefaultColumnNames.Features, dataView, dataProcessPipeline, 1);

            // STEP 3: Set the training algorithm, then create and config the modelBuilder                            
            var trainer = mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: DefaultColumnNames.Label, featureColumnName: DefaultColumnNames.Features);
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            ITransformer trainedModel = trainingPipeline.Fit(testTrainSplit.TrainSet);

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testTrainSplit.TestSet);
            var metrics = mlContext.BinaryClassification.Evaluate(data:predictions, label: DefaultColumnNames.Label, score: DefaultColumnNames.Score);

            ConsoleHelper.PrintBinaryClassificationMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file

            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(trainedModel, fs);

            Console.WriteLine("The model is saved to {0}", ModelPath);

            return trainedModel;
        }

        // (OPTIONAL) Try/test a single prediction by loding the model from the file, first.
        private static void TestSinglePrediction(MLContext mlContext)
        {         
            SentimentIssue sampleStatement = new SentimentIssue { Text = "This is a very rude movie" };

            ITransformer trainedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream);
            }

            // Create prediction engine related to the loaded trained model
            var predEngine= trainedModel.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(mlContext);

            //Score
            var resultprediction = predEngine.Predict(sampleStatement);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Text: {sampleStatement.Text} | Prediction: {(Convert.ToBoolean(resultprediction.Prediction) ? "Negative" : "Nice")} sentiment | Probability: {resultprediction.Probability} ");
            Console.WriteLine($"==================================================");
        }

        private static void DownloadDataIfNotExists(string dataPath)
        {
            var downloadPath = "https://aka.ms/tlc-resources/benchmarks/WikiDetoxAnnotated160kRows.tsv";
            var folderPath = GetAbsolutePath(BaseDatasetsRelativePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(dataPath))
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(new Uri(downloadPath), dataPath);
                }
            }
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath , relativePath);

            return fullPath;
        }
    }
}