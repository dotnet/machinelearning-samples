using System;
using System.IO;

using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using MulticlassClassification_Iris.DataStructures;

namespace MulticlassClassification_Iris
{
    public static partial class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../Data";
        private static string TrainDataPath = $"{BaseDatasetsLocation}/iris-train.txt";
        private static string TestDataPath = $"{BaseDatasetsLocation}/iris-test.txt";

        private static string BaseModelsPath = @"../../../../MLModels";
        private static string ModelPath = $"{BaseModelsPath}/IrisClassificationModel.zip";

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
            var textLoader = mlContext.Data.CreateTextReader(
                                                                new TextLoader.Arguments()
                                                                {
                                                                    Separator = "\t",
                                                                    HasHeader = true,
                                                                    Column = new[]
                                                                    {
                                                                        new TextLoader.Column("Label", DataKind.R4, 0),
                                                                        new TextLoader.Column("SepalLength", DataKind.R4, 1),
                                                                        new TextLoader.Column("SepalWidth", DataKind.R4, 2),
                                                                        new TextLoader.Column("PetalLength", DataKind.R4, 3),
                                                                        new TextLoader.Column("PetalWidth", DataKind.R4, 4),
                                                                    }
                                                                });

            var trainingDataView = textLoader.Read(TrainDataPath);
            var testDataView = textLoader.Read(TestDataPath);

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Concatenate("Features", "SepalLength",
                                                                                   "SepalWidth",
                                                                                   "PetalLength",
                                                                                   "PetalWidth").AppendCacheCheckpoint(mlContext);

            // STEP 3: Set the training algorithm, then append the trainer to the pipeline  
            var trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(labelColumn: "Label", featureColumn: "Features");
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
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label", "Score");

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

            Console.WriteLine($"Actual: setosa.     Predicted probability: setosa:      {resultprediction2.Score[0]:0.####}");
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
    }
}