using System;
using System.IO;

using Microsoft.ML;
using Microsoft.ML.Data;
using MulticlassClassification_Iris.DataStructures;

namespace MulticlassClassification_Iris
{
    public static partial class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsRelativePath = @"../../../../Data";
        private static string TrainDataRelativePath = $"{BaseDatasetsRelativePath}/iris-train.txt";
        private static string TestDataRelativePath = $"{BaseDatasetsRelativePath}/iris-test.txt";

        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);
        private static string TestDataPath = GetAbsolutePath(TestDataRelativePath);

        private static string BaseModelsRelativePath = @"../../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/IrisClassificationModel.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        private static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 0);

            //1.
            BuildTrainEvaluateAndSaveModel(mlContext);

            //2.
            TestSomePredictions(mlContext);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static void BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // STEP 1: Common data loading configuration
            var trainingDataView = mlContext.Data.LoadFromTextFile<IrisData>(TrainDataPath, hasHeader: true);
            var testDataView = mlContext.Data.LoadFromTextFile<IrisData>(TestDataPath, hasHeader: true);
            

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Concatenate(DefaultColumnNames.Features, nameof(IrisData.SepalLength),
                                                                                   nameof(IrisData.SepalWidth),
                                                                                   nameof(IrisData.PetalLength),
                                                                                   nameof(IrisData.PetalWidth))
                                                                       .AppendCacheCheckpoint(mlContext); 
                                                                       // Use in-memory cache for small/medium datasets to lower training time. 
                                                                       // Do NOT use it (remove .AppendCacheCheckpoint()) when handling very large datasets. 

            // STEP 3: Set the training algorithm, then append the trainer to the pipeline  
            var trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(labelColumnName: DefaultColumnNames.Label, featureColumnName: DefaultColumnNames.Features);
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet

            //Measure training time
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Console.WriteLine("=============== Training the model ===============");
            ITransformer trainedModel = trainingPipeline.Fit(trainingDataView);

            //Stop measuring time
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"***** Training time: {elapsedMs/1000} seconds *****");


            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions, DefaultColumnNames.Label, DefaultColumnNames.Score);

            Common.ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(trainedModel, fs);

            Console.WriteLine("The model is saved to {0}", ModelPath);
        }

        private static void TestSomePredictions(MLContext mlContext)
        {
            //Test Classification Predictions with some hard-coded samples 

            ITransformer trainedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream);
            }

            // Create prediction engine related to the loaded trained model
            var predEngine = trainedModel.CreatePredictionEngine<IrisData, IrisPrediction>(mlContext);

            //Score sample 1
            var resultprediction1 = predEngine.Predict(SampleIrisData.Iris1);

            Console.WriteLine($"Actual: setosa.     Predicted probability: setosa:      {resultprediction1.Score[0]:0.####}");
            Console.WriteLine($"                                           versicolor:  {resultprediction1.Score[1]:0.####}");
            Console.WriteLine($"                                           virginica:   {resultprediction1.Score[2]:0.####}");
            Console.WriteLine();

            //Score sample 2
            var resultprediction2 = predEngine.Predict(SampleIrisData.Iris2);

            Console.WriteLine($"Actual: Virginica.     Predicted probability: setosa:      {resultprediction2.Score[0]:0.####}");
            Console.WriteLine($"                                           versicolor:  {resultprediction2.Score[1]:0.####}");
            Console.WriteLine($"                                           virginica:   {resultprediction2.Score[2]:0.####}");
            Console.WriteLine();

            //Score sample 3
            var resultprediction3 = predEngine.Predict(SampleIrisData.Iris3);

            Console.WriteLine($"Actual: setosa.     Predicted probability: setosa:      {resultprediction3.Score[0]:0.####}");
            Console.WriteLine($"                                           versicolor:  {resultprediction3.Score[1]:0.####}");
            Console.WriteLine($"                                           virginica:   {resultprediction3.Score[2]:0.####}");
            Console.WriteLine();

        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
