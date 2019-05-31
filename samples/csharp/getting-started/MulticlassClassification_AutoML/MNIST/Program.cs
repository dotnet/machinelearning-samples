using System;
using System.IO;
using System.Linq;
using Common;
using Microsoft.ML;
using Microsoft.ML.AutoML;
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
                ConsoleHelper.ConsoleWriteHeader("=============== Running AutoML experiment ===============");
                Console.WriteLine($"Running AutoML multiclass classification experiment for {ExperimentTime} seconds...");
                ExperimentResult<MulticlassClassificationMetrics> experimentResult = mlContext.Auto()
                    .CreateMulticlassClassificationExperiment(ExperimentTime)
                    .Execute(trainData, "Number", progressHandler: progressHandler);

                // Print top models found by AutoML
                Console.WriteLine();
                PrintTopModels(experimentResult);

                // STEP 4: Evaluate the model and print metrics
                ConsoleHelper.ConsoleWriteHeader("===== Evaluating model's accuracy with test data =====");
                RunDetail<MulticlassClassificationMetrics> bestRun = experimentResult.BestRun;
                ITransformer trainedModel = bestRun.Model;
                var predictions = trainedModel.Transform(testData);
                var metrics = mlContext.MulticlassClassification.Evaluate(data:predictions, labelColumnName: "Number", scoreColumnName: "Score");
                ConsoleHelper.PrintMulticlassClassificationMetrics(bestRun.TrainerName, metrics);

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
        /// Print top models from AutoML experiment.
        /// </summary>
        private static void PrintTopModels(ExperimentResult<MulticlassClassificationMetrics> experimentResult)
        {
            // Get top few runs ranked by accuracy
            var topRuns = experimentResult.RunDetails
                .Where(r => r.ValidationMetrics != null && !double.IsNaN(r.ValidationMetrics.MicroAccuracy))
                .OrderByDescending(r => r.ValidationMetrics.MicroAccuracy).Take(3);

            Console.WriteLine("Top models ranked by accuracy --");
            ConsoleHelper.PrintMulticlassClassificationMetricsHeader();
            for (var i = 0; i < topRuns.Count(); i++)
            {
                var run = topRuns.ElementAt(i);
                ConsoleHelper.PrintIterationMetrics(i + 1, run.TrainerName, run.ValidationMetrics, run.RuntimeInSeconds);
            }
        }

        private static void TestSomePredictions(MLContext mlContext)
        {
            ConsoleHelper.ConsoleWriteHeader("=============== Testing prediction engine ===============");

            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<InputData, OutputData>(trainedModel);

            // Get the key value mapping for Number to Score index
            var keyValues = default(VBuffer<float>);
            trainedModel.GetOutputSchema(modelInputSchema)["Number"].GetKeyValues<float>(ref keyValues);
            var keys = keyValues.Items().ToDictionary(x => (int)x.Value, x => x.Key);

            //InputData data1 = SampleMNISTData.MNIST1;
            var predictedResult1 = predEngine.Predict(SampleMNISTData.MNIST1);

            Console.WriteLine($"Actual: 1     Predicted probability:       zero:  {predictedResult1.Score[keys[0]]:0.####}");
            Console.WriteLine($"                                           One :  {predictedResult1.Score[keys[1]]:0.####}");
            Console.WriteLine($"                                           two:   {predictedResult1.Score[keys[2]]:0.####}");
            Console.WriteLine($"                                           three: {predictedResult1.Score[keys[3]]:0.####}");
            Console.WriteLine($"                                           four:  {predictedResult1.Score[keys[4]]:0.####}");
            Console.WriteLine($"                                           five:  {predictedResult1.Score[keys[5]]:0.####}");
            Console.WriteLine($"                                           six:   {predictedResult1.Score[keys[6]]:0.####}");
            Console.WriteLine($"                                           seven: {predictedResult1.Score[keys[7]]:0.####}");
            Console.WriteLine($"                                           eight: {predictedResult1.Score[keys[8]]:0.####}");
            Console.WriteLine($"                                           nine:  {predictedResult1.Score[keys[9]]:0.####}");
            Console.WriteLine();

            var predictedResult2 = predEngine.Predict(SampleMNISTData.MNIST2);

            Console.WriteLine($"Actual: 7     Predicted probability:       zero:  {predictedResult2.Score[keys[0]]:0.####}");
            Console.WriteLine($"                                           One :  {predictedResult2.Score[keys[1]]:0.####}");
            Console.WriteLine($"                                           two:   {predictedResult2.Score[keys[2]]:0.####}");
            Console.WriteLine($"                                           three: {predictedResult2.Score[keys[3]]:0.####}");
            Console.WriteLine($"                                           four:  {predictedResult2.Score[keys[4]]:0.####}");
            Console.WriteLine($"                                           five:  {predictedResult2.Score[keys[5]]:0.####}");
            Console.WriteLine($"                                           six:   {predictedResult2.Score[keys[6]]:0.####}");
            Console.WriteLine($"                                           seven: {predictedResult2.Score[keys[7]]:0.####}");
            Console.WriteLine($"                                           eight: {predictedResult2.Score[keys[8]]:0.####}");
            Console.WriteLine($"                                           nine:  {predictedResult2.Score[keys[9]]:0.####}");
            Console.WriteLine();
        }
    }
}
