using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.ML;
using TitanicSurvivalConsoleApp.DataStructures;

namespace TitanicSurvivalConsoleApp
{

    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../Data";
        private static string TrainDataPath = $"{BaseDatasetsLocation}/titanic-train.csv";
        private static string TestDataPath = $"{BaseDatasetsLocation}/titanic-test.csv";

        private static string BaseModelsPath = @"../../../../MLModels";
        private static string ModelPath = $"{BaseModelsPath}/TitanicModel.zip";

        private static void Main(string[] args)
        {
            //Create ML Context with seed for repeteable/deterministic results
            MLContext mlContext = new MLContext(seed: 0);

            // STEP 1: Create/Train, Evaluate and save the model
            CreateTrainAndEvaluateModel(mlContext);

            // STEP 2: Make a single test prediction
            TestSinglePrediction(mlContext);

            Common.ConsoleHelper.ConsolePressAnyKey();
        }

        public static void CreateTrainAndEvaluateModel(MLContext mlContext)
        {
            // STEP 1: Common data loading configuration
            DataLoader dataLoader = new DataLoader(mlContext);
            var trainingDataView = dataLoader.GetDataView(TrainDataPath);
            var testDataView = dataLoader.GetDataView(TestDataPath);

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessor = new DataProcessor(mlContext);
            var dataProcessPipeline = dataProcessor.DataProcessPipeline;

            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            Common.ConsoleHelper.PeekDataViewInConsole<TitanicData>(mlContext, trainingDataView, dataProcessPipeline, 2);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 2);

            // STEP 3: Set the training algorithm, then create and config the modelBuilder    
            // FastTreeBinaryClassifier is an algorithm that will be used to train the model.
            // It has three hyperparameters for tuning decision tree performance. 
            //pipeline.Add(new FastTreeBinaryClassifier());// {NumLeaves = 5, NumTrees = 5, MinDocumentsInLeafs = 2});
            var modelBuilder = new Common.ModelBuilder<TitanicData, TitanicPrediction>(mlContext, dataProcessPipeline);
            var trainer = mlContext.BinaryClassification.Trainers.FastTree(label: "Label", features: "Features", numLeaves:10, numTrees:5, minDatapointsInLeafs:10);
            modelBuilder.AddTrainer(trainer);

            // STEP 4: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            modelBuilder.Train(trainingDataView);

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var metrics = modelBuilder.EvaluateBinaryClassificationModel(testDataView, "Label", "Score");
            Common.ConsoleHelper.PrintBinaryClassificationMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file
            Console.WriteLine("=============== Saving the model to a file ===============");
            modelBuilder.SaveModelAsFile(ModelPath);
        }


        private static void TestSinglePrediction(MLContext mlContext)
        {
            // (OPTIONAL) Try/test a single prediction by loding the model from the file, first.
            TitanicData sampleTitanicPassengerData = TestTitanicData.Passenger;

            var modelScorer = new Common.ModelScorer<TitanicData, TitanicPrediction>(mlContext);
            modelScorer.LoadModelFromZipFile(ModelPath);
            var resultprediction = modelScorer.PredictSingle(sampleTitanicPassengerData);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($" Did this passenger survive?   Actual: Yes   Predicted: {(resultprediction.Survived ? "Yes" : "No")} with {resultprediction.Probability * 100}% probability");
            Console.WriteLine($"==================================================");

        }

    }
    }