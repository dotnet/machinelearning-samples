using Common;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Ranking.DataStructures;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Ranking
{
    class Program
    {
        const string AssetsPath = @"../../../Assets";
        const string TrainDatasetUrl = "https://aka.ms/mlnet-resources/benchmarks/MSLRWeb10KTrain720kRows.tsv";
        const string ValidationDatasetUrl = "https://aka.ms/mlnet-resources/benchmarks/MSLRWeb10KValidate240kRows.tsv";
        const string TestDatasetUrl = "https://aka.ms/mlnet-resources/benchmarks/MSLRWeb10KTest240kRows.tsv";

        readonly static string InputPath = Path.Combine(AssetsPath, "Input");
        readonly static string OutputPath = Path.Combine(AssetsPath, "Output");
        readonly static string TrainDatasetPath = Path.Combine(InputPath, "MSLRWeb10KTrain720kRows.tsv");
        readonly static string ValidationDatasetPath = Path.Combine(InputPath, "MSLRWeb10KValidate240kRows.tsv");
        readonly static string TestDatasetPath = Path.Combine(InputPath, "MSLRWeb10KTest240kRows.tsv");
        readonly static string ModelPath = Path.Combine(OutputPath, "RankingModel.zip");

        private static uint ExperimentTime = 60;

        private static RankingData sampleRankingData = null;

        static void Main(string[] args)
        {
            var mlContext = new MLContext(seed: 0);

            // Create, train, evaluate and save a model
            BuildTrainEvaluateAndSaveModel(mlContext);

            // Make a single test prediction loading the model from .ZIP file
            TestSinglePrediction(mlContext);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static ITransformer BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // STEP 1: Download and load the data
            GetData(InputPath, OutputPath, TrainDatasetPath, TrainDatasetUrl, TestDatasetUrl, TestDatasetPath,
                ValidationDatasetUrl, ValidationDatasetPath);

            IDataView trainDataView = mlContext.Data.LoadFromTextFile<RankingData>(TrainDatasetPath, hasHeader: true, separatorChar: '\t');
            IDataView testDataView = mlContext.Data.LoadFromTextFile<RankingData>(TestDatasetPath, hasHeader: true, separatorChar: '\t');

            // STEP 2: Display first few rows of training data
            ConsoleHelper.ShowDataViewInConsole(mlContext, trainDataView);

            // STEP 3: Initialize our user-defined progress handler that AutoML will 
            // invoke after each model it produces and evaluates.
            var progressHandler = new RankingExperimentProgressHandler();

            // STEP 4: Run AutoML ranking experiment
            ConsoleHelper.ConsoleWriteHeader("=============== Running AutoML experiment ===============");
            Console.WriteLine($"Running AutoML ranking experiment for {ExperimentTime} seconds...");

            ExperimentResult<RankingMetrics> experimentResult = mlContext.Auto()
                .CreateRankingExperiment(ExperimentTime)
                .Execute(
                    trainData: trainDataView, 
                    validationData: testDataView,
                    progressHandler: progressHandler);

            // Print top models found by AutoML
            Console.WriteLine();
            PrintTopModels(experimentResult);

            // STEP 5: Evaluate the model and print metrics
            ConsoleHelper.ConsoleWriteHeader("=============== Evaluating model's nDCG with test data ===============");
            RunDetail<RankingMetrics> bestRun = experimentResult.BestRun;

            ITransformer trainedModel = bestRun.Model;
            var predictions = trainedModel.Transform(testDataView);

            // Get a row for single prediction
            sampleRankingData = mlContext.Data.CreateEnumerable<RankingData>(mlContext.Data.TakeRows(testDataView, 1), reuseRowObject: false)
                .FirstOrDefault();

            var metrics = mlContext.Ranking.Evaluate(predictions);

            ConsoleHelper.PrintRankingMetrics(bestRun.TrainerName, metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            mlContext.Model.Save(trainedModel, trainDataView.Schema, ModelPath);

            Console.WriteLine("The model is saved to {0}", ModelPath);

            return trainedModel;
        }

        private static void TestSinglePrediction(MLContext mlContext)
        {
            ConsoleHelper.ConsoleWriteHeader("=============== Testing prediction engine ===============");

            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);
            Console.WriteLine($"=============== Loaded Model OK  ===============");

            // Create prediction engine related to the loaded trained model
            var predictionEngine = mlContext.Model.CreatePredictionEngine<RankingData, RankingPrediction>(trainedModel, inputSchema: modelInputSchema);

            var predictionResult = predictionEngine.Predict(sampleRankingData);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Prediction: {predictionResult.Score}");
            Console.WriteLine($"==================================================");
        }

        private static void GetData(string inputPath, string outputPath, string trainDatasetPath, string trainDatasetUrl,
            string testDatasetUrl, string testDatasetPath, string validationDatasetUrl, string validationDatasetPath)
        {
            Console.WriteLine("===== Prepare data =====\n");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            if (!Directory.Exists(inputPath))
            {
                Directory.CreateDirectory(inputPath);
            }

            if (!File.Exists(trainDatasetPath))
            {
                Console.WriteLine("===== Download the train dataset - this may take several minutes =====\n");
                using (var client = new WebClient())
                {
                    client.DownloadFile(trainDatasetUrl, TrainDatasetPath);
                }
            }

            if (!File.Exists(validationDatasetPath))
            {
                Console.WriteLine("===== Download the validation dataset - this may take several minutes =====\n");
                using (var client = new WebClient())
                {
                    client.DownloadFile(validationDatasetUrl, validationDatasetPath);
                }
            }

            if (!File.Exists(testDatasetPath))
            {
                Console.WriteLine("===== Download the test dataset - this may take several minutes =====\n");
                using (var client = new WebClient())
                {
                    client.DownloadFile(testDatasetUrl, testDatasetPath);
                }
            }

            Console.WriteLine("===== Download is finished =====\n");
        }

        private static void PrintTopModels(ExperimentResult<RankingMetrics> experimentResult)
        {
            // Get top few runs ranked by nDCG
            var topRuns = experimentResult.RunDetails
                .Where(r => r.ValidationMetrics != null && !double.IsNaN(r.ValidationMetrics.NormalizedDiscountedCumulativeGains.Average()))
                .OrderByDescending(r => r.ValidationMetrics.NormalizedDiscountedCumulativeGains.Average()).Take(3);

            Console.WriteLine("Top models ranked by nDCG --");
            ConsoleHelper.PrintRankingMetricsHeader();
            for (var i = 0; i < topRuns.Count(); i++)
            {
                var run = topRuns.ElementAt(i);
                ConsoleHelper.PrintIterationMetrics(i + 1, run.TrainerName, run.ValidationMetrics, run.RuntimeInSeconds);
            }
        }
    }
}
