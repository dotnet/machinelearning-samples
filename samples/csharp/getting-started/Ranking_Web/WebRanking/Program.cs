using Microsoft.ML;
using WebRanking.Common;
using WebRanking.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace WebRanking
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

        static void Main(string[] args)
        {
            // Create a common ML.NET context.
            // Seed set to any number so you have a deterministic environment for repeateable results.
            MLContext mlContext = new MLContext(seed: 0);

            try
            {
                PrepareData(InputPath, OutputPath, TrainDatasetPath, TrainDatasetUrl, TestDatasetUrl, TestDatasetPath, ValidationDatasetUrl, ValidationDatasetPath);

                // Create the pipeline using the training data's schema; the validation and testing data have the same schema.
                IDataView trainData = mlContext.Data.LoadFromTextFile<SearchResultData>(TrainDatasetPath, separatorChar: '\t', hasHeader: true);
                IEstimator<ITransformer> pipeline = CreatePipeline(mlContext, trainData);

                // Train the model on the training dataset. To perform training you need to call the Fit() method.
                Console.WriteLine("===== Train the model on the training dataset =====\n");
                ITransformer model = pipeline.Fit(trainData);

                // Evaluate the model using the metrics from the validation dataset; you would then retrain and reevaluate the model until the desired metrics are achieved.
                Console.WriteLine("===== Evaluate the model's result quality with the validation data =====\n");
                IDataView validationData = mlContext.Data.LoadFromTextFile<SearchResultData>(ValidationDatasetPath, separatorChar: '\t', hasHeader: false);
                EvaluateModel(mlContext, model, validationData);

                // Combine the training and validation datasets.
                var validationDataEnum = mlContext.Data.CreateEnumerable<SearchResultData>(validationData, false);
                var trainDataEnum = mlContext.Data.CreateEnumerable<SearchResultData>(trainData, false);
                var trainValidationDataEnum = validationDataEnum.Concat<SearchResultData>(trainDataEnum);
                IDataView trainValidationData = mlContext.Data.LoadFromEnumerable<SearchResultData>(trainValidationDataEnum);

                // Train the model on the train + validation dataset.
                Console.WriteLine("===== Train the model on the training + validation dataset =====\n");
                model = pipeline.Fit(trainValidationData);

                // Evaluate the model using the metrics from the testing dataset; you do this only once and these are your final metrics.
                Console.WriteLine("===== Evaluate the model's result quality with the testing data =====\n");
                IDataView testData = mlContext.Data.LoadFromTextFile<SearchResultData>(TestDatasetPath, separatorChar: '\t', hasHeader: false);
                EvaluateModel(mlContext, model, testData);

                // Combine the training, validation, and testing datasets.
                var testDataEnum = mlContext.Data.CreateEnumerable<SearchResultData>(testData, false);
                var allDataEnum = trainValidationDataEnum.Concat<SearchResultData>(testDataEnum);
                IDataView allData = mlContext.Data.LoadFromEnumerable<SearchResultData>(allDataEnum);

                // Retrain the model on all of the data, train + validate + test.
                Console.WriteLine("===== Train the model on the training + validation + test dataset =====\n");
                model = pipeline.Fit(allData);

                // Save and consume the model to perform predictions.
                // Normally, you would use new incoming data; however, for the purposes of this sample, we'll reuse the test data to show how to do predictions.
                ConsumeModel(mlContext, model, ModelPath, testData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.Write("Done!");
            Console.ReadLine();
        }

        static void PrepareData(string inputPath, string outputPath, string trainDatasetPath, string trainDatasetUrl, 
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

        static IEstimator<ITransformer> CreatePipeline(MLContext mlContext, IDataView dataView)
        {
            const string FeaturesVectorName = "Features";

            Console.WriteLine("===== Set up the trainer =====\n");

            // Specify the columns to include in the feature input data.
            var featureCols = dataView.Schema.AsQueryable()
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

            return trainerPipeline;
        }

        static void EvaluateModel(MLContext mlContext, ITransformer model, IDataView data)
        {
            // Use the model to perform predictions on the test data.
            IDataView predictions = model.Transform(data);

            Console.WriteLine("===== Use metrics for the data using NDCG@3 =====\n");

            // Evaluate the metrics for the data using NDCG; by default, metrics for the up to 3 search results in the query are reported (e.g. NDCG@3).
            ConsoleHelper.EvaluateMetrics(mlContext, predictions);

            // Evaluate metrics for up to 10 search results (e.g. NDCG@10).
            // TO CHECK:
            //Console.WriteLine("===== Use metrics for the data using NDCG@10 =====\n");
            //ConsoleHelper.EvaluateMetrics(mlContext, predictions, 10);
        }

        static void ConsumeModel(MLContext mlContext, ITransformer model, string modelPath, IDataView data)
        {
            Console.WriteLine("===== Save the model =====\n");

            // Save the model
            mlContext.Model.Save(model, null, modelPath);

            Console.WriteLine("===== Consume the model =====\n");

            // Load the model to perform predictions with it.
            DataViewSchema predictionPipelineSchema;
            ITransformer predictionPipeline = mlContext.Model.Load(modelPath, out predictionPipelineSchema);

            // Predict rankings.
            IDataView predictions = predictionPipeline.Transform(data);

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
