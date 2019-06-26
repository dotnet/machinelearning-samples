using Microsoft.ML;
using Microsoft.ML.Trainers.LightGbm;
using PersonalizedRanking.Common;
using PersonalizedRanking.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using static Microsoft.ML.DataOperationsCatalog;

namespace PersonalizedRanking
{
    class Program
    {
        const string AssetsPath = @"../../../Assets";
        readonly static string TrainDatasetPath = Path.Combine(AssetsPath, "InputData_Train.csv");
        readonly static string TestDatasetPath = Path.Combine(AssetsPath, "InputData_Test.csv");
        readonly static string ModelPath = Path.Combine(AssetsPath, "RankingModel.csv");

        readonly static string OriginalDatasetPath = Path.Combine(AssetsPath, "Train.csv");
        readonly static string OriginalExampleDatasetPath = Path.Combine(AssetsPath, "Test.csv");

        static void Main(string[] args)
        {
            // Create a common ML.NET context.
            // Seed set to any number so you have a deterministic environment for repeateable results.
            MLContext mlContext = new MLContext(seed: 0);

            try
            {
                PrepDatasets(mlContext, AssetsPath, OriginalDatasetPath, TrainDatasetPath, TestDatasetPath);

                var model = TrainModel(mlContext, TrainDatasetPath, ModelPath);

                EvaluateModel(mlContext, model, TestDatasetPath);

                ConsumeModel(mlContext, ModelPath, OriginalExampleDatasetPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }

        static void PrepDatasets(MLContext mlContext, string assetPath, string originalDatasetPath, string trainDatasetPath, string testDatasetPath)
        {
            const string DatasetUrl = "https://www.kaggle.com/c/expedia-personalized-sort/download/data.zip";

                if (!File.Exists(trainDatasetPath) || !File.Exists(testDatasetPath))
                {
                    if (!File.Exists(originalDatasetPath))
                    {
                        throw new InvalidOperationException($"This samples requires the Expedia dataset.  Please ensure that you have downloaded and extracted the contents of the .zip file to the following directory: {assetPath}. The .zip file can be downloaded from here: {DatasetUrl}");
                    }

                    Console.WriteLine("===== Prepare the testing/training datasets =====");

                    // Load dataset using TextLoader by specifying the type name that holds the data's schema to be mapped with datasets.
                    IDataView data = mlContext.Data.LoadFromTextFile<HotelData>(originalDatasetPath, separatorChar: ',', hasHeader: true);

                    Console.WriteLine("===== Label the dataset with ideal ranking value =====");

                    // Create an Estimator and use a custom mapper to transform label hotel instances to values 0, 1, or 2.
                    IEstimator<ITransformer> dataPipeline = mlContext.Transforms.CustomMapping(Mapper.GetLabelMapper(mlContext, data), null);

                    // To transform the data, call the Fit() method.
                    ITransformer dataTransformer = dataPipeline.Fit(data);
                    IDataView labeledData = dataTransformer.Transform(data);

                    Console.WriteLine("===== Split the data into testing/training datasets =====");

                    // When splitting the data, 20% is held for the test dataset.
                    // To avoid data leakage, the GroupId (e.g. search\query id) is specified as the samplingKeyColumnName.  
                    // This ensures that if two or more hotel instances share the same GroupId, that they are guaranteed to appear in the same subset of data (train or test).
                    TrainTestData trainTestData = mlContext.Data.TrainTestSplit(labeledData, testFraction: 0.2, samplingKeyColumnName: nameof(HotelData.GroupId), seed: 1);
                    IDataView trainData = trainTestData.TrainSet;
                    IDataView testData = trainTestData.TestSet;

                    Console.WriteLine("===== Save the testing/training datasets =====");

                    // Save the test dataset to a file to make it faster to load in subsequent runs.
                    using (var fileStream = File.Create(trainDatasetPath))
                    {
                        mlContext.Data.SaveAsText(trainData, fileStream, separatorChar: ',', headerRow: true, schema: true);
                    }

                    // Save the train dataset to a file to make it faster to load in subsequent runs.
                    using (var fileStream = File.Create(testDatasetPath))
                    {
                        mlContext.Data.SaveAsText(testData, fileStream, separatorChar: ',', headerRow: true, schema: true);
                    }
                }
        }

        static ITransformer TrainModel(MLContext mlContext, string trainDatasetPath, string modelPath)
        {
            const string FeaturesVectorName = "Features";

            Console.WriteLine("===== Load the training dataset =====");

            // Load the training dataset.
            IDataView trainData = mlContext.Data.LoadFromTextFile<HotelData>(trainDatasetPath, separatorChar: ',', hasHeader: true);

            Console.WriteLine("===== Set up the trainer =====");

            // Specify the columns to include in the feature input data.
            var featureCols = trainData.Schema.AsQueryable()
                .Select(s => s.Name)
                .Where(c =>
                    c == nameof(HotelData.Price_USD) ||
                    c == nameof(HotelData.Promotion_Flag) ||
                    c == nameof(HotelData.Prop_Id) ||
                    c == nameof(HotelData.Prop_Brand) ||
                    c == nameof(HotelData.Prop_Review_Score))
                 .ToArray();

            // Set trainer options.
            LightGbmRankingTrainer.Options options = new LightGbmRankingTrainer.Options();
            options.CustomGains = new int[] { 0, 1, 5 };
            options.RowGroupColumnName = nameof(HotelData.GroupId);
            options.LabelColumnName = nameof(HotelData.Label);
            options.FeatureColumnName = FeaturesVectorName;

            // Create an Estimator and transform the data:
            // 1. Concatenate the feature columns into a single Features vector.
            // 2. Create a key type for the label input data by using the value to key transform.
            // 3. Create a key type for the group input data by using a hash transform.
            IEstimator<ITransformer> dataPipeline = mlContext.Transforms.Concatenate(FeaturesVectorName, featureCols)
                .Append(mlContext.Transforms.Conversion.MapValueToKey(nameof(HotelData.Label)))
                .Append(mlContext.Transforms.Conversion.Hash(nameof(HotelData.GroupId), nameof(HotelData.GroupId), numberOfBits: 20));

            // Set the LightGBM LambdaRank trainer.
            IEstimator<ITransformer> trainer = mlContext.Ranking.Trainers.LightGbm(options);
            IEstimator<ITransformer> trainerPipeline = dataPipeline.Append(trainer);

            Console.WriteLine("===== Train the model =====");

            // Training the model is a process of running the chosen algorithm on the given data. To perform training you need to call the Fit() method.
            ITransformer model = trainerPipeline.Fit(trainData);

            Console.WriteLine("===== Save the model =====");

            // Save the model
            mlContext.Model.Save(model, trainData.Schema, modelPath);

            return model;
        }

        static void EvaluateModel(MLContext mlContext, ITransformer model, string testDatasetPath)
        {
            Console.WriteLine("===== Evaluate the model's result quality with test data =====");

            // Load the test data and use the model to perform predictions on the test data.
            IDataView testData = mlContext.Data.LoadFromTextFile<HotelData>(testDatasetPath, separatorChar: ',', hasHeader: true);
            IDataView predictions = model.Transform(testData);

            // Evaluate the metrics for the data using NDCG; by default, metrics for the up to 3 search results in the query are reported (e.g. NDCG@3).
            ConsoleHelper.EvaluateMetrics(mlContext, predictions);

            // Evaluate metrics for up to 10 search results (e.g. NDCG@10).
            ConsoleHelper.EvaluateMetrics(mlContext, predictions, 10);
        }

        public static void ConsumeModel(MLContext mlContext, string modelPath, string exampleDatasetPath)
        {
            Console.WriteLine("===== Consume the model =====");

            DataViewSchema predictionPipelineSchema;
            ITransformer predictionPipeline = mlContext.Model.Load(modelPath, out predictionPipelineSchema);

            // Load example data and use the model to perform predictions on it.
            IDataView exampleData = mlContext.Data.LoadFromTextFile<HotelData>(exampleDatasetPath, separatorChar: ',', hasHeader: true);

            // Predict rankings.
            IDataView predictions = predictionPipeline.Transform(exampleData);

            // In the predictions, get the scores of the hotel search results included in the first query (e.g. group).
            IEnumerable<HotelPrediction> hotelQueries = mlContext.Data.CreateEnumerable<HotelPrediction>(predictions, reuseRowObject: false);
            var firstGroupId = hotelQueries.First<HotelPrediction>().GroupId;
            IEnumerable<HotelPrediction> firstGroupPredictions = hotelQueries.Take(50).Where(p => p.GroupId == firstGroupId).OrderByDescending(p => p.Score).ToList();

            // The individual scores themselves are NOT a useful measure of result quality; instead, they are only useful as a relative measure to other scores in the group. 
            // The scores are used to determine the ranking where a higher score indicates a higher ranking versus another candidate result.
            ConsoleHelper.PrintScores(firstGroupPredictions);
        }
    }
}
