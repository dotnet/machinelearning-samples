using Microsoft.ML;
using PersonalizedRanking.Common;
using PersonalizedRanking.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace PersonalizedRanking
{
    class Program
    {
        const string AssetsPath = @"../../../Assets";
        const string TrainDatasetUrl = "https://aka.ms/mlnet-resources/benchmarks/MSLRWeb10KTrain720kRows.tsv";
        const string TestDatasetUrl = "https://aka.ms/mlnet-resources/benchmarks/MSLRWeb10KTest240kRows.tsv";

        readonly static string InputPath = Path.Combine(AssetsPath, "Input");
        readonly static string OutputPath = Path.Combine(AssetsPath, "Output");
        readonly static string TrainDatasetPath = Path.Combine(InputPath, "MSLRWeb10KTrain720kRows.tsv");
        readonly static string TestDatasetPath = Path.Combine(InputPath, "MSLRWeb10KTest240kRows.tsv");
        readonly static string ModelPath = Path.Combine(OutputPath, "RankingModel.zip");

        static void Main(string[] args)
        {
            // Create a common ML.NET context.
            // Seed set to any number so you have a deterministic environment for repeateable results.
            MLContext mlContext = new MLContext(seed: 0);

            try
            {
                PrepareData(InputPath, OutputPath, TrainDatasetPath, TrainDatasetUrl, TestDatasetUrl, TestDatasetPath);

                var model = TrainModel(mlContext, TrainDatasetPath, ModelPath);

                EvaluateModel(mlContext, model, TestDatasetPath);

                ConsumeModel(mlContext, ModelPath, TestDatasetPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }

        static void PrepareData(string inputPath, string outputPath, string trainDatasetPath, string trainDatasetUrl, string testDatasetUrl, string testDatasetPath)
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

        static ITransformer TrainModel(MLContext mlContext, string trainDatasetPath, string modelPath)
        {
            const string FeaturesVectorName = "Features";

            Console.WriteLine("===== Load the training dataset =====\n");

            // Load the training dataset.
            IDataView trainData = mlContext.Data.LoadFromTextFile<SearchResultData>(trainDatasetPath, separatorChar: '\t', hasHeader: true);

            Console.WriteLine("===== Set up the trainer =====\n");

            // Specify the columns to include in the feature input data.
            var featureCols = trainData.Schema.AsQueryable()
                .Select(s => s.Name)
                .Where(c =>
                    c != nameof(SearchResultData.Label) &&
                    c != nameof(SearchResultData.GroupId))
                 .ToArray();

            // Create an Estimator and transform the data:
            // 1. Concatenate the feature columns into a single Features vector.
            // 2. Create a key type for the label input data by using the value to key transform.
            // 3. Create a key type for the group input data by using a hash transform.
            IEstimator<ITransformer> dataPipeline = mlContext.Transforms.Concatenate(FeaturesVectorName, featureCols)
                .Append(mlContext.Transforms.Conversion.MapValueToKey(nameof(SearchResultData.Label)))
                .Append(mlContext.Transforms.Conversion.Hash(nameof(SearchResultData.GroupId), nameof(SearchResultData.GroupId), numberOfBits: 20));

            // Set the LightGBM LambdaRank trainer.
            IEstimator<ITransformer> trainer = mlContext.Ranking.Trainers.LightGbm(labelColumnName: nameof(SearchResultData.Label), featureColumnName: FeaturesVectorName, rowGroupColumnName: nameof(SearchResultData.GroupId));
            IEstimator<ITransformer> trainerPipeline = dataPipeline.Append(trainer);

            Console.WriteLine("===== Train the model =====\n");

            // Training the model is a process of running the chosen algorithm on the given data. To perform training you need to call the Fit() method.
            ITransformer model = trainerPipeline.Fit(trainData);
            IDataView transformedTrainData = model.Transform(trainData);
;
            Console.WriteLine("===== Save the model =====\n");

            // Save the model
            mlContext.Model.Save(model, null, modelPath);

            return model;
        }

        static void EvaluateModel(MLContext mlContext, ITransformer model, string testDatasetPath)
        {
            Console.WriteLine("===== Evaluate the model's result quality with test data =====\n");

            // Load the test data and use the model to perform predictions on the test data.
            IDataView testData = mlContext.Data.LoadFromTextFile<SearchResultData>(testDatasetPath, separatorChar: '\t', hasHeader: false);
            IDataView predictions = model.Transform(testData);

            Console.WriteLine("===== Use metrics for the data using NDCG@3 =====\n");

            // Evaluate the metrics for the data using NDCG; by default, metrics for the up to 3 search results in the query are reported (e.g. NDCG@3).
            ConsoleHelper.EvaluateMetrics(mlContext, predictions);

            Console.WriteLine("===== Use metrics for the data using NDCG@10 =====\n");

            // Evaluate metrics for up to 10 search results (e.g. NDCG@10).
            ConsoleHelper.EvaluateMetrics(mlContext, predictions, 10);
        }

        public static void ConsumeModel(MLContext mlContext, string modelPath, string testDatasetPath)
        {
            Console.WriteLine("===== Consume the model =====\n");

            // Load test data and use the model to perform predictions on it.
            IDataView testData = mlContext.Data.LoadFromTextFile<SearchResultData>(testDatasetPath, separatorChar: '\t', hasHeader: false);

            // Load the model.
            DataViewSchema predictionPipelineSchema;
            ITransformer predictionPipeline = mlContext.Model.Load(modelPath, out predictionPipelineSchema);

            // Predict rankings.
            IDataView predictions = predictionPipeline.Transform(testData);

            // In the predictions, get the scores of the search results included in the first query (e.g. group).
            IEnumerable<SearchResultPrediction> searchQueries = mlContext.Data.CreateEnumerable<SearchResultPrediction>(predictions, reuseRowObject: false);
            var firstGroupId = searchQueries.First<SearchResultPrediction>().GroupId;
            IEnumerable<SearchResultPrediction> firstGroupPredictions = searchQueries.Take(100).Where(p => p.GroupId == firstGroupId).OrderByDescending(p => p.Score).ToList();

            // The individual scores themselves are NOT a useful measure of result quality; instead, they are only useful as a relative measure to other scores in the group. 
            // The scores are used to determine the ranking where a higher score indicates a higher ranking versus another candidate result.
            ConsoleHelper.PrintScores(firstGroupPredictions);
        }
    }
}
