﻿using System;
using System.Threading.Tasks;
using System.IO;

// Requires following NuGet packages
// NuGet package -> Microsoft.Extensions.Configuration
// NuGet package -> Microsoft.Extensions.Configuration.Json
using Microsoft.Extensions.Configuration;

using Microsoft.ML;
using Microsoft.ML.Transforms.Conversions;
using Microsoft.ML.Core.Data;

using Common;
using GitHubLabeler.DataStructures;
using Microsoft.ML.Data;

namespace GitHubLabeler
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../Data";
        private static string DataSetLocation = $"{BaseDatasetsLocation}/corefx-issues-train.tsv";      

        private static string BaseModelsPath = @"../../../../MLModels";
        private static string ModelFilePathName = $"{BaseModelsPath}/GitHubLabelerModel.zip";

        public enum MyTrainerStrategy : int { SdcaMultiClassTrainer = 1, OVAAveragedPerceptronTrainer = 2 };

        public static IConfiguration Configuration { get; set; }
        private static async Task Main(string[] args)
        {
            SetupAppConfiguration();

            //1. ChainedBuilderExtensions and Train the model
            BuildAndTrainModel(DataSetLocation, ModelFilePathName, MyTrainerStrategy.SdcaMultiClassTrainer);

            //2. Try/test to predict a label for a single hard-coded Issue
            TestSingleLabelPrediction(ModelFilePathName);

            //3. Predict Issue Labels and apply into a real GitHub repo
            // (Comment the next line if no real access to GitHub repo) 
            await PredictLabelsAndUpdateGitHub(ModelFilePathName);

            Common.ConsoleHelper.ConsolePressAnyKey();
        }

        public static void BuildAndTrainModel(string DataSetLocation, string ModelPath, MyTrainerStrategy selectedStrategy)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 0);

            // STEP 1: Common data loading configuration
            var trainingDataView = mlContext.Data.ReadFromTextFile<GitHubIssue>(DataSetLocation, hasHeader: true, separatorChar:'\t', supportSparse: false);
             
            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: DefaultColumnNames.Label,inputColumnName:nameof(GitHubIssue.Area))
                            .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "TitleFeaturized",inputColumnName:nameof(GitHubIssue.Title)))
                            .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "DescriptionFeaturized", inputColumnName: nameof(GitHubIssue.Description)))
                            .Append(mlContext.Transforms.Concatenate(outputColumnName:DefaultColumnNames.Features, "TitleFeaturized", "DescriptionFeaturized"))
                            .AppendCacheCheckpoint(mlContext);  
                            // Use in-memory cache for small/medium datasets to lower training time. 
                            // Do NOT use it (remove .AppendCacheCheckpoint()) when handling very large datasets.

            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            Common.ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline, 2);

            // STEP 3: Create the selected training algorithm/trainer
            IEstimator<ITransformer> trainer = null; 
            switch(selectedStrategy)
            {
                case MyTrainerStrategy.SdcaMultiClassTrainer:                 
                     trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(DefaultColumnNames.Label, 
                                                                                                          DefaultColumnNames.Features);
                     break;
                case MyTrainerStrategy.OVAAveragedPerceptronTrainer:
                {
                    // Create a binary classification trainer.
                    var averagedPerceptronBinaryTrainer = mlContext.BinaryClassification.Trainers.AveragedPerceptron(DefaultColumnNames.Label,
                                                                                                                     DefaultColumnNames.Features,
                                                                                                                     numIterations: 10);
                    // Compose an OVA (One-Versus-All) trainer with the BinaryTrainer.
                    // In this strategy, a binary classification algorithm is used to train one classifier for each class, "
                    // which distinguishes that class from all other classes. Prediction is then performed by running these binary classifiers, "
                    // and choosing the prediction with the highest confidence score.
                    trainer = mlContext.MulticlassClassification.Trainers.OneVersusAll(averagedPerceptronBinaryTrainer);
                        
                    break;
                }
                default:
                    break;
            }

            //Set the trainer/algorithm and map label to value (original readable state)
            var trainingPipeline = dataProcessPipeline.Append(trainer)
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue(DefaultColumnNames.PredictedLabel));

            // STEP 4: Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
            // in order to evaluate and get the model's accuracy metrics

            Console.WriteLine("=============== Cross-validating to get model's accuracy metrics ===============");

            //Measure cross-validation time
            var watchCrossValTime = System.Diagnostics.Stopwatch.StartNew();

            var crossValidationResults = mlContext.MulticlassClassification.CrossValidate(data:trainingDataView, estimator:trainingPipeline, numFolds: 6, labelColumn:DefaultColumnNames.Label);

            //Stop measuring time
            watchCrossValTime.Stop();
            long elapsedMs = watchCrossValTime.ElapsedMilliseconds;
            Console.WriteLine($"Time Cross-Validating: {elapsedMs} miliSecs");
           
            ConsoleHelper.PrintMulticlassClassificationFoldsAverageMetrics(trainer.ToString(), crossValidationResults);

            // STEP 5: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");

            //Measure training time
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var trainedModel = trainingPipeline.Fit(trainingDataView);

            //Stop measuring time
            watch.Stop();
            long elapsedCrossValMs = watch.ElapsedMilliseconds;

            Console.WriteLine($"Time Training the model: {elapsedCrossValMs} miliSecs");

            // (OPTIONAL) Try/test a single prediction with the "just-trained model" (Before saving the model)
            GitHubIssue issue = new GitHubIssue() { ID = "Any-ID", Title = "WebSockets communication is slow in my machine", Description = "The WebSockets communication used under the covers by SignalR looks like is going slow in my development machine.." };
            // Create prediction engine related to the loaded trained model
            var predEngine = trainedModel.CreatePredictionEngine<GitHubIssue, GitHubIssuePrediction>(mlContext);
            //Score
            var prediction = predEngine.Predict(issue);
            Console.WriteLine($"=============== Single Prediction just-trained-model - Result: {prediction.Area} ===============");
            //

            // STEP 6: Save/persist the trained model to a .ZIP file
            Console.WriteLine("=============== Saving the model to a file ===============");
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(trainedModel, fs);

            Common.ConsoleHelper.ConsoleWriteHeader("Training process finalized");
        }

        private static void TestSingleLabelPrediction(string modelFilePathName)
        {
            var labeler = new Labeler(modelPath: ModelFilePathName);
            labeler.TestPredictionForSingleIssue();
        }

        private static async Task PredictLabelsAndUpdateGitHub(string ModelPath)
        {
            Console.WriteLine(".............Retrieving Issues from GITHUB repo, predicting label/s and assigning predicted label/s......");

            var token = Configuration["GitHubToken"];
            var repoOwner = Configuration["GitHubRepoOwner"]; //IMPORTANT: This can be a GitHub User or a GitHub Organization
            var repoName = Configuration["GitHubRepoName"];

            if (string.IsNullOrEmpty(token) || token == "YOUR - GUID - GITHUB - TOKEN" ||
                string.IsNullOrEmpty(repoOwner) || repoOwner == "YOUR-REPO-USER-OWNER-OR-ORGANIZATION" ||
                string.IsNullOrEmpty(repoName) || repoName == "YOUR-REPO-SINGLE-NAME" )
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Error: please configure the credentials in the appsettings.json file");
                Console.ReadLine();
                return;
            }

            //This "Labeler" class could be used in a different End-User application (Web app, other console app, desktop app, etc.) 
            var labeler = new Labeler(ModelPath, repoOwner, repoName, token);

            await labeler.LabelAllNewIssuesInGitHubRepo();

            Console.WriteLine("Labeling completed");
            Console.ReadLine();
        }

        private static void SetupAppConfiguration()
        {
            var builder = new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }
    }
}
