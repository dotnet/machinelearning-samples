using Common;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using Ranking.DataStructures;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

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

        // Runtime should allow for the sweeping to plateau, which begins near iteration 60
        private static uint ExperimentTime = 600;

        static void Main(string[] args)
        {
            var mlContext = new MLContext(seed: 0);

            // Create, train, evaluate and save a model
            (var model, var predictions) = BuildTrainEvaluateAndSaveModel(mlContext);

            // Make a single test prediction loading the model from .ZIP file
            TestSinglePrediction(mlContext, predictions);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static (ITransformer, IDataView) BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // STEP 1: Download and load the data
            GetData(InputPath, OutputPath, TrainDatasetPath, TrainDatasetUrl, TestDatasetUrl, TestDatasetPath,
                ValidationDatasetUrl, ValidationDatasetPath);

            //ColumnInferenceResults columnInference = mlContext.Auto().InferColumns(TrainDatasetPath, labelColumnIndex: 0, 
            //    separatorChar: '\t', hasHeader: true, groupColumns: false, allowSparse: true);

            var textLoaderOptions = new TextLoader.Options
            {
                Separators = new[] { '\t' },
                HasHeader = true,
                Columns = new[]
                {
                    new TextLoader.Column("Label", DataKind.Single, 0),
                    new TextLoader.Column("GroupId", DataKind.Int32, 1),
                    new TextLoader.Column("Features", DataKind.Single, 2, 133),
                }
            };

            TextLoader textLoader = mlContext.Data.CreateTextLoader(textLoaderOptions);
            IDataView trainDataView = textLoader.Load(TrainDatasetPath);
            IDataView validationDataView = textLoader.Load(ValidationDatasetPath);
            IDataView testDataView = textLoader.Load(TestDatasetPath);

            // STEP 2: Display first few rows of training data
            ConsoleHelper.ShowDataViewInConsole(mlContext, trainDataView);

            // STEP 3: Initialize our user-defined progress handler that AutoML will 
            // invoke after each model it produces and evaluates.
            var progressHandler = new RankingExperimentProgressHandler();

            // STEP 4: Run AutoML ranking experiment
            ConsoleHelper.ConsoleWriteHeader("=============== Running AutoML experiment ===============");
            Console.WriteLine($"Running AutoML ranking experiment for {ExperimentTime} seconds...");

            var experimentSettings = new RankingExperimentSettings
            {
                MaxExperimentTimeInSeconds = ExperimentTime,
                OptimizingMetric = RankingMetric.Ndcg,
                OptimizationMetricTruncationLevel = 10
            };

            ExperimentResult<RankingMetrics> experimentResult = mlContext.Auto()
                .CreateRankingExperiment(experimentSettings)
                .Execute(
                    trainData: trainDataView,
                    validationData: validationDataView,
                    progressHandler: progressHandler);

            // Print top models found by AutoML
            Console.WriteLine("\n===== Evaluating model's NDCG (on validation data) =====");
            PrintTopModels(experimentResult, experimentSettings.OptimizationMetricTruncationLevel);

            var rankingEvaluatorOptions = new RankingEvaluatorOptions
            {
                DcgTruncationLevel = Math.Min(10, (int)experimentSettings.OptimizationMetricTruncationLevel * 2)
            };

            Console.WriteLine("\n===== Evaluating model's NDCG (on test data) =====");
            IDataView predictions = experimentResult.BestRun.Model.Transform(testDataView);
            var metrics = mlContext.Ranking.Evaluate(predictions, rankingEvaluatorOptions);
            ConsoleHelper.PrintRankingMetrics(experimentResult.BestRun.TrainerName, metrics, experimentSettings.OptimizationMetricTruncationLevel);

            // STEP 5: Refit the model with all available data
            // Re-fit best pipeline on train and validation data, to produce 
            // a model that is trained on as much data as is available while
            // still having test data for the final estimate of how well the
            // model will do in production.
            Console.WriteLine("\n===== Refitting on train+valid and evaluating model's NDCG (on test data) =====");
            var trainPlusValidationDataView = textLoader.Load(new MultiFileSource(TrainDatasetPath, ValidationDatasetPath));
            var refitModel = experimentResult.BestRun.Estimator.Fit(trainPlusValidationDataView);
            IDataView predictionsRefitOnTrainPlusValidation = refitModel.Transform(testDataView);
            var metricsRefitOnTrainPlusValidation = mlContext.Ranking.Evaluate(predictionsRefitOnTrainPlusValidation, rankingEvaluatorOptions);
            ConsoleHelper.PrintRankingMetrics(experimentResult.BestRun.TrainerName, metricsRefitOnTrainPlusValidation, experimentSettings.OptimizationMetricTruncationLevel);

            // STEP 6: Refit the model with all available data
            // Re-fit best pipeline again on train, validation, and test data, to 
            // produce a model that is trained on as much data as is available.
            // This is the final model that can be deployed to production.
            // No metrics are printed since we no longer have an independent
            // scoring dataset.
            Console.WriteLine("\n===== Refitting on train+valid+test to get the final model to launch to production =====");
            var trainPlusValidationPlusTestDataView = textLoader.Load(new MultiFileSource(TrainDatasetPath, ValidationDatasetPath, TestDatasetPath));
            var refitModelOnTrainValidTest = experimentResult.BestRun.Estimator.Fit(trainPlusValidationPlusTestDataView);
            
            // STEP 7: Save/persist the trained model to a .ZIP file
            mlContext.Model.Save(refitModelOnTrainValidTest, trainDataView.Schema, ModelPath);

            Console.WriteLine("The model is saved to {0}", ModelPath);

            return (refitModelOnTrainValidTest, predictionsRefitOnTrainPlusValidation);
        }

        private static void TestSinglePrediction(MLContext mlContext, IDataView predictions)
        {
            ConsoleHelper.ConsoleWriteHeader("=============== Testing prediction engine ===============");

            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);
            Console.WriteLine($"=============== Loaded Model OK  ===============");

            // In the predictions, get the scores of the search results included in the first query (e.g. group).
            var searchQueries = mlContext.Data.CreateEnumerable<RankingPrediction>(predictions, reuseRowObject: false);
            var firstGroupId = searchQueries.First().GroupId;
            var firstGroupPredictions = searchQueries.Take(100).Where(p => p.GroupId == firstGroupId).OrderByDescending(p => p.Score).ToList();

            // The individual scores themselves are NOT a useful measure of result quality; instead, they are only useful as a relative measure to other scores in the group. 
            // The scores are used to determine the ranking where a higher score indicates a higher ranking versus another candidate result.
            foreach (var prediction in firstGroupPredictions)
            {
                Console.WriteLine($"GroupId: {prediction.GroupId}, Score: {prediction.Score}");
            }
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
                Console.WriteLine("===== Downloading the train dataset - this may take several minutes =====\n");
                using (var client = new WebClient())
                {
                    client.DownloadFile(trainDatasetUrl, TrainDatasetPath);
                }
            }

            if (!File.Exists(validationDatasetPath))
            {
                Console.WriteLine("===== Downloading the validation dataset - this may take several minutes =====\n");
                using (var client = new WebClient())
                {
                    client.DownloadFile(validationDatasetUrl, validationDatasetPath);
                }
            }

            if (!File.Exists(testDatasetPath))
            {
                Console.WriteLine("===== Downloading the test dataset - this may take several minutes =====\n");
                using (var client = new WebClient())
                {
                    client.DownloadFile(testDatasetUrl, testDatasetPath);
                }
            }

            Console.WriteLine("===== Download is finished =====\n");
        }

        private static void PrintTopModels(ExperimentResult<RankingMetrics> experimentResult, uint optimizationMetricTruncationLevel)
        {
            // Get top few runs ordered by NDCG
            var topRuns = experimentResult.RunDetails
                .Where(r => r.ValidationMetrics != null && !double.IsNaN(r.ValidationMetrics.NormalizedDiscountedCumulativeGains[(int)optimizationMetricTruncationLevel - 1]))
                .OrderByDescending(r => r.ValidationMetrics.NormalizedDiscountedCumulativeGains[(int)optimizationMetricTruncationLevel - 1]).Take(5);

            Console.WriteLine($"Top models ordered by NDCG@{optimizationMetricTruncationLevel}");
            ConsoleHelper.PrintRankingMetricsHeader();
            for (var i = 0; i < topRuns.Count(); i++)
            {
                var run = topRuns.ElementAt(i);
                ConsoleHelper.PrintIterationMetrics(i + 1, run.TrainerName, run.ValidationMetrics, run.RuntimeInSeconds);
            }
        }
    }
}
