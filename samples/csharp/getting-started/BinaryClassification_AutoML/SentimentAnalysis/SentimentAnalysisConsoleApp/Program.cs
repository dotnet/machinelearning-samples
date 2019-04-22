using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Auto;
using Microsoft.ML.Data;
using SentimentAnalysisConsoleApp.DataStructures;
using Common;
using static Microsoft.ML.DataOperationsCatalog;

namespace SentimentAnalysisConsoleApp
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static readonly string BaseDatasetsRelativePath = @"../../../../Data";
        private static readonly string TrainDataRelativePath = $"{BaseDatasetsRelativePath}/wikipedia-detox-250-line-data.tsv";
        private static readonly string TestDataRelativePath = $"{BaseDatasetsRelativePath}/wikipedia-detox-250-line-test.tsv";
        private static string TrainDataPath = GetAbsolutePath(TrainDataRelativePath);
        private static string TestDataPath = GetAbsolutePath(TestDataRelativePath);

        private static readonly string BaseModelsRelativePath = @"../../../../MLModels";
        private static readonly string ModelRelativePath = $"{BaseModelsRelativePath}/SentimentModel.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        private static uint ExperimentTime = 60;
        static void Main(string[] args)
        {
            //Create MLContext to be shared across the model creation workflow objects 
            //Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 1);

            // Create, Train, Evaluate and Save a model
            BuildTrainEvaluateAndSaveModel(mlContext);
            Common.ConsoleHelper.ConsoleWriteHeader("=============== End of training process ===============");

            // Make a single test prediction loading the model from .ZIP file
            TestSinglePrediction(mlContext);

            Common.ConsoleHelper.ConsoleWriteHeader("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();

        }

        private static ITransformer BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // // STEP 1: Load data
            IDataView trainDataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(TrainDataPath, hasHeader: true);
            IDataView testDataView = mlContext.Data.LoadFromTextFile<SentimentIssue>(TestDataPath, hasHeader: true);


            // STEP 2: Display first few rows of the test data        
            // (OPTIONAL) Show data (such as 2 records) in training DataView 
            ConsoleHelper.ShowDataViewInConsole(mlContext, trainDataView);

            //TODO: VectorColumnData - look into this without use of dataProcessPipeline
            //ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", dataView, dataProcessPipeline, 1);

            // STEP 3: Auto featurize, auto train and auto hyperparameter tune                        
            Console.WriteLine($"Running AutoML binary classification experiment for {ExperimentTime} seconds...");
            IEnumerable<RunDetails<BinaryClassificationMetrics>> runDetails = mlContext.Auto()
                                                                             .CreateBinaryClassificationExperiment(ExperimentTime)
                                                                             .Execute(trainDataView);

            // STEP 4: Evaluate the model and show metrics
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            RunDetails<BinaryClassificationMetrics> best = runDetails.Best();
            ITransformer trainedModel = best.Model;
            var predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.BinaryClassification.EvaluateNonCalibrated(data:predictions, labelColumnName: "Label", scoreColumnName: "Score");

            //TODO: Create method to print noncalibrated metrics and print top 5 performing models 
            //ConsoleHelper.PrintBinaryClassificationMetrics(best.TrainerName.ToString(), metrics);

            // STEP 5: Save/persist the trained model to a .ZIP file
            mlContext.Model.Save(trainedModel,  trainDataView.Schema, ModelPath);

            Console.WriteLine("The model is saved to {0}", ModelPath);

            return trainedModel;
        }

        // (OPTIONAL) Try/test a single prediction by loading the model from the file, first.
        private static void TestSinglePrediction(MLContext mlContext)
        {         
            SentimentIssue sampleStatement = new SentimentIssue { Text = "This is a very rude movie" };
            
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);       
            Console.WriteLine($"=============== Loaded Model OK  ===============");               

            // Create prediction engine related to the loaded trained model
            var predEngine= mlContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(trainedModel);
            Console.WriteLine($"=============== Created Prediction Engine OK  ==============="); 
            //Score
            var resultprediction = predEngine.Predict(sampleStatement);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Text: {sampleStatement.Text} | Prediction: {(Convert.ToBoolean(resultprediction.Prediction) ? "Toxic" : "Non Toxic")} sentiment | Probability of being toxic: {resultprediction.Probability} ");
            Console.WriteLine($"==================================================");
        }
        
        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath , relativePath);

            return fullPath;
        }
    }
}
