﻿using System;
using System.IO;
using Microsoft.ML.Core.Data;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Data;

using SentimentAnalysisConsoleApp.DataStructures;
using Microsoft.ML.Transforms.Text;
using System.Data;
using Common;


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
            TextLoader textLoader = mlContext.Data.CreateTextReader(                                                        
                                                        columns:new[]
                                                                    {
                                                                    new TextLoader.Column("Label", DataKind.Bool, 0),
                                                                    new TextLoader.Column("Text", DataKind.Text, 1)
                                                                    },                                                     
                                                        hasHeader:true,
                                                        separatorChar:'\t'
                                                        );

            IDataView trainingDataView = textLoader.Read(TrainDataPath);
            IDataView testDataView = textLoader.Read(TestDataPath);

            // STEP 2: Common data process configuration with pipeline data transformations          
            var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText("Text", "Features");

            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            ConsoleHelper.PeekDataViewInConsole<SentimentIssue>(mlContext, trainingDataView, dataProcessPipeline, 2);
            ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 1);

            // STEP 3: Set the training algorithm, then create and config the modelBuilder                            
            var trainer = mlContext.BinaryClassification.Trainers.FastTree(labelColumn: "Label", featureColumn: "Features");
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            ITransformer trainedModel = trainingPipeline.Fit(trainingDataView);

            // STEP 5: Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label", "Score");

            ConsoleHelper.PrintBinaryClassificationMetrics(trainer.ToString(), metrics);

            // STEP 6: Save/persist the trained model to a .ZIP file

            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(trainedModel, fs);

            Console.WriteLine("The model is saved to {0}", ModelPath);

            return trainedModel;
        }

        // (OPTIONAL) Try/test a single prediction by loding the model from the file, first.
        private static void TestSinglePrediction(MLContext mlContext)
        {
            
            SentimentIssue sampleStatement = new SentimentIssue { Text = "This is a very rude movie" };

            ITransformer trainedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream);
            }

            // Create prediction engine related to the loaded trained model
            var predEngine= trainedModel.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(mlContext);

            //Score
            var resultprediction = predEngine.Predict(sampleStatement);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Text: {sampleStatement.Text} | Prediction: {(Convert.ToBoolean(resultprediction.Prediction) ? "Toxic" : "Nice")} sentiment | Probability: {resultprediction.Probability} ");
            Console.WriteLine($"==================================================");
            //
        }
    }
}