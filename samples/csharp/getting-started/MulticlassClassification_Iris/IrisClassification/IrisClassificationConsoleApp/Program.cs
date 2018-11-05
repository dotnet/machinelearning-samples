using System;
using System.IO;

using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML;
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
            DataLoader dataLoader = new DataLoader(mlContext);
            var trainingDataView = dataLoader.GetDataView(TrainDataPath);
            var testDataView = dataLoader.GetDataView(TestDataPath);

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessor = new DataProcessor(mlContext);
            var dataProcessPipeline = dataProcessor.DataProcessPipeline;

            // (OPTIONAL) Peek data (such as 5 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            Common.ConsoleHelper.PeekDataViewInConsole<IrisData>(mlContext, trainingDataView, dataProcessPipeline, 5);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 5);

            // STEP 3: Set the training algorithm, then create and config the modelBuilder                            
            var modelBuilder = new Common.ModelBuilder<IrisData, IrisPrediction>(mlContext, dataProcessPipeline);
            // We apply our selected Trainer 
            var trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(label: "Label", features: "Features");
            modelBuilder.AddTrainer(trainer);

            // STEP 4: Train the model fitting to the DataSet
            //The pipeline is trained on the dataset that has been loaded and transformed.
            Console.WriteLine("=============== Training the model ===============");
            modelBuilder.Train(trainingDataView);

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var metrics = modelBuilder.EvaluateMultiClassClassificationModel(testDataView, "Label");
            Common.ConsoleHelper.PrintMultiClassClassificationMetrics("StochasticDualCoordinateAscent", metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            Console.WriteLine("=============== Saving the model to a file ===============");
            modelBuilder.SaveModelAsFile(ModelPath);
        }

        private static void TestSomePredictions(MLContext mlContext)
        {
            //Test Classification Predictions with some hard-coded samples 

            var modelScorer = new Common.ModelScorer<IrisData, IrisPrediction>(mlContext);
            modelScorer.LoadModelFromZipFile(ModelPath);

            var prediction = modelScorer.PredictSingle(SampleIrisData.Iris1);
            Console.WriteLine($"Actual: setosa.     Predicted probability: setosa:      {prediction.Score[0]:0.####}");
            Console.WriteLine($"                                           versicolor:  {prediction.Score[1]:0.####}");
            Console.WriteLine($"                                           virginica:   {prediction.Score[2]:0.####}");
            Console.WriteLine();

            prediction = modelScorer.PredictSingle(SampleIrisData.Iris2);
            Console.WriteLine($"Actual: virginica.  Predicted probability: setosa:      {prediction.Score[0]:0.####}");
            Console.WriteLine($"                                           versicolor:  {prediction.Score[1]:0.####}");
            Console.WriteLine($"                                           virginica:   {prediction.Score[2]:0.####}");
            Console.WriteLine();

            prediction = modelScorer.PredictSingle(SampleIrisData.Iris3);
            Console.WriteLine($"Actual: versicolor. Predicted probability: setosa:      {prediction.Score[0]:0.####}");
            Console.WriteLine($"                                           versicolor:  {prediction.Score[1]:0.####}");
            Console.WriteLine($"                                           virginica:   {prediction.Score[2]:0.####}");
            Console.WriteLine();

        }
    }
}