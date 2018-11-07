using System;
using System.IO;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Core.Data;
using Microsoft.ML;
using Microsoft.ML.Trainers;

using SentimentAnalysisConsoleApp.DataStructures;
using Microsoft.ML.Transforms.Text;

namespace SentimentAnalysisConsoleApp
{

    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../Data";
        private static string TrainDataPath = $"{BaseDatasetsLocation}/wikipedia-detox-250-line-data.tsv";
        private static string TestDataPath = $"{BaseDatasetsLocation}/wikipedia-detox-250-line-test.tsv";

        private static string BaseModelsPath = @"../../../../MLModels";
        private static string ModelPath = $"{BaseModelsPath}/SentimentModel.zip";

        static void Main(string[] args)
        {
            //Create MLContext to be shared across the model creation workflow objects 
            //Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 1);

            // Create, Train, Evaluate and Save a model
            BuildTrainEvaluateAndSaveModel(mlContext);
            Common.ConsoleHelper.ConsoleWriteHeader("=============== End of training processh ===============");

            // Make a single test prediction loding the model from .ZIP file
            TestSinglePrediction(mlContext);

            Common.ConsoleHelper.ConsoleWriteHeader("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();

        }

        private static ITransformer BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // STEP 1: Common data loading configuration
            DataLoader dataLoader = new DataLoader(mlContext);
            var trainingDataView = dataLoader.GetDataView(TrainDataPath);
            var testDataView = dataLoader.GetDataView(TestDataPath);

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessor = new DataProcessor(mlContext);
            var dataProcessPipeline = dataProcessor.DataProcessPipeline;

            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            Common.ConsoleHelper.PeekDataViewInConsole<SentimentIssue>(mlContext, trainingDataView, dataProcessPipeline, 2);
            //Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 2);

            // STEP 3: Set the training algorithm, then create and config the modelBuilder                            
            var modelBuilder = new Common.ModelBuilder<SentimentIssue, SentimentPrediction>(mlContext, dataProcessPipeline);
            var trainer = mlContext.BinaryClassification.Trainers.FastTree(label: "Label", features: "Features");
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

            return modelBuilder.TrainedModel;
        }

        private static void TestSinglePrediction(MLContext mlContext)
        {
            // (OPTIONAL) Try/test a single prediction by loding the model from the file, first.
            SentimentIssue sampleStatement = new SentimentIssue { Text = "This is a very rude movie" };
            var modelScorer = new Common.ModelScorer<SentimentIssue, SentimentPrediction>(mlContext);
            modelScorer.LoadModelFromZipFile(ModelPath);
            var resultprediction = modelScorer.PredictSingle(sampleStatement);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Text: {sampleStatement.Text} | Prediction: {(Convert.ToBoolean(resultprediction.Prediction) ? "Toxic" : "Nice")} sentiment | Probability: {resultprediction.Probability} ");
            Console.WriteLine($"==================================================");
            //
        }
    }
}