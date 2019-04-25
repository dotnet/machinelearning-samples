using System;
using System.IO;
using System.Linq;
using Common;
using Microsoft.ML;
using Microsoft.ML.Auto;
using Microsoft.ML.Data;
using MNIST.DataStructures;

namespace MNIST
{
    class Program
    {
        private static string BaseDatasetsRelativePath = @"Data";
        private static string TrainDataRelativePath = $"{BaseDatasetsRelativePath}/optdigits-train.csv";
        private static string TestDataRelativePath = $"{BaseDatasetsRelativePath}/optdigits-test.csv";
        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);
        private static string TestDataPath = GetAbsolutePath(TestDataRelativePath);

        private static string BaseModelsRelativePath = @"../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/Model.zip";
        private static string ModelPath = GetAbsolutePath(ModelRelativePath);
        
        private static uint ExperimentTime = 60;

        static void Main(string[] args)
        {
            MLContext mlContext = new MLContext();
            Train(mlContext);
            TestSomePredictions(mlContext);

            Console.WriteLine("Hit any key to finish the app");
            Console.ReadKey();
        }

        public static void Train(MLContext mlContext)
        {
            try
            {
                // STEP 1: Load the data
                var trainData = mlContext.Data.LoadFromTextFile(path: TrainDataPath,
                        columns : new[] 
                        {
                            new TextLoader.Column(nameof(InputData.PixelValues), DataKind.Single, 0, 63),
                            new TextLoader.Column("Number", DataKind.Single, 64)
                        },
                        hasHeader : false,
                        separatorChar : ','
                        );
                
                var testData = mlContext.Data.LoadFromTextFile(path: TestDataPath,
                        columns: new[]
                        {
                            new TextLoader.Column(nameof(InputData.PixelValues), DataKind.Single, 0, 63),
                            new TextLoader.Column("Number", DataKind.Single, 64)
                        },
                        hasHeader: false,
                        separatorChar: ','
                        );

                // STEP 2: Initialize our user-defined progress handler that AutoML will 
                // invoke after each model it produces and evaluates.
                var progressHandler = new MulticlassExperimentProgressHandler();

                // STEP 3: Run an AutoML multiclass classification experiment
                Console.WriteLine("=============== Training the model ===============");
                Console.WriteLine($"Running AutoML multiclass classification experiment for {ExperimentTime} seconds...");
                ExperimentResult<MulticlassClassificationMetrics> experimentResult = mlContext.Auto()
                    .CreateMulticlassClassificationExperiment(ExperimentTime)
                    .Execute(trainData, "Number", progressHandler: progressHandler);

                // STEP 4: Evaluate the model and print metrics
                Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
                RunDetail<MulticlassClassificationMetrics> bestRun = experimentResult.BestRun;
                ITransformer trainedModel = bestRun.Model;
                var predictions = trainedModel.Transform(testData);
                var metrics = mlContext.MulticlassClassification.Evaluate(data:predictions, labelColumnName: "Number", scoreColumnName: "Score");
                ConsoleHelper.PrintMulticlassClassificationMetrics(bestRun.TrainerName, metrics);

                // Print top models found by AutoML
                PrintTopModels(experimentResult);

                // STEP 5: Save/persist the trained model to a .ZIP file
                mlContext.Model.Save(trainedModel, trainData.Schema, ModelPath);

                Console.WriteLine("The model is saved to {0}", ModelPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        /// <summary>
        /// Prints top models from AutoML experiment.
        /// </summary>
        private static void PrintTopModels(ExperimentResult<MulticlassClassificationMetrics> experimentResult)
        {
            // Get top few runs ranked by accuracy
            var topRuns = experimentResult.RunDetails.OrderByDescending(r => r.ValidationMetrics.MicroAccuracy).Take(3);

            Console.WriteLine($"Top models ranked by accuracy --");
            ConsoleHelper.PrintMulticlassClassificationMetricsHeader();
            for (var i = 0; i < topRuns.Count(); i++)
            {
                var run = topRuns.ElementAt(i);
                ConsoleHelper.PrintIterationMetrics(i, run.TrainerName, run.ValidationMetrics, run.RuntimeInSeconds);
            }
        }

        private static void TestSomePredictions(MLContext mlContext)
        {
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<InputData, OutputData>(trainedModel);

            //InputData data1 = SampleMNISTData.MNIST1;
            var resultprediction1 = predEngine.Predict(SampleMNISTData.MNIST1);

            Console.WriteLine($"Actual: 7     Predicted probability:       zero:  {resultprediction1.Score[0]:0.####}");
            Console.WriteLine($"                                           One :  {resultprediction1.Score[1]:0.####}");
            Console.WriteLine($"                                           two:   {resultprediction1.Score[2]:0.####}");
            Console.WriteLine($"                                           three: {resultprediction1.Score[3]:0.####}");
            Console.WriteLine($"                                           four:  {resultprediction1.Score[4]:0.####}");
            Console.WriteLine($"                                           five:  {resultprediction1.Score[5]:0.####}");
            Console.WriteLine($"                                           six:   {resultprediction1.Score[6]:0.####}");
            Console.WriteLine($"                                           seven: {resultprediction1.Score[7]:0.####}");
            Console.WriteLine($"                                           eight: {resultprediction1.Score[8]:0.####}");
            Console.WriteLine($"                                           nine:  {resultprediction1.Score[9]:0.####}");
            Console.WriteLine();
                       
            var resultprediction2 = predEngine.Predict(SampleMNISTData.MNIST2);

            Console.WriteLine($"Actual: 1     Predicted probability:       zero:  {resultprediction2.Score[0]:0.####}");
            Console.WriteLine($"                                           One :  {resultprediction2.Score[1]:0.####}");
            Console.WriteLine($"                                           two:   {resultprediction2.Score[2]:0.####}");
            Console.WriteLine($"                                           three: {resultprediction2.Score[3]:0.####}");
            Console.WriteLine($"                                           four:  {resultprediction2.Score[4]:0.####}");
            Console.WriteLine($"                                           five:  {resultprediction2.Score[5]:0.####}");
            Console.WriteLine($"                                           six:   {resultprediction2.Score[6]:0.####}");
            Console.WriteLine($"                                           seven: {resultprediction2.Score[7]:0.####}");
            Console.WriteLine($"                                           eight: {resultprediction2.Score[8]:0.####}");
            Console.WriteLine($"                                           nine:  {resultprediction2.Score[9]:0.####}");
            Console.WriteLine();
        }
    }
}
