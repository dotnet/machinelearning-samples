using System;
using System.Configuration;
using System.Threading.Tasks;
using System.IO;

// Requires following NuGet packages
// NuGet package -> Microsoft.Extensions.Configuration
// NuGet package -> Microsoft.Extensions.Configuration.Json
using Microsoft.Extensions.Configuration;

using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.Conversions;

using Common;
using GitHubLabeler.DataStructures;
using Microsoft.ML.Trainers.Online;

namespace GitHubLabeler
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../Data";
        private static string DataSetLocation = $"{BaseDatasetsLocation}/corefx-issues-train.tsv";
        //private static string DataSetLocation => Path.Combine(AppPath, "Data", "corefx-issues-train.tsv");

        private static string BaseModelsPath = @"../../../../MLModels";
        private static string ModelFilePathName = $"{BaseModelsPath}/GitHubLabelerModel.zip";
        //private static string ModelPath => Path.Combine(AppPath, "MLModels", "GitHubLabelerModel.zip");

        public static IConfiguration Configuration { get; set; }
        private static async Task Main(string[] args)
        {
            SetupAppConfiguration();

            //1. ChainedBuilderExtensions and Train the model
            BuildAndTrainModel(DataSetLocation, ModelFilePathName);

            //2. Try/test to predict a label for a single hard-coded Issue
            TestSingleLabelPrediction(ModelFilePathName);

            //3. Predict Issue Labels and apply into a real GitHub repo
            await PredictLabelsAndUpdateGitHub(ModelFilePathName);
        }

        public static void BuildAndTrainModel(string DataSetLocation, string ModelPath)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 0);

            // STEP 1: Common data loading configuration
            DataLoader dataLoader = new DataLoader(mlContext);
            var trainingDataView = dataLoader.GetDataView(DataSetLocation);

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessor = new DataProcessor(mlContext);
            var dataProcessPipeline = dataProcessor.DataProcessPipeline;

            // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
            Common.ConsoleHelper.PeekDataViewInConsole<GitHubIssue>(mlContext, trainingDataView, dataProcessPipeline, 2);
            //Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 2);

            // STEP 3: Set the selected training algorithm into the modelBuilder            
            var modelBuilder = new Common.ModelBuilder<GitHubIssue, GitHubIssuePrediction>(mlContext, dataProcessPipeline);
            var trainer = mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent("Label", "Features");
            modelBuilder.AddTrainer(trainer);
            modelBuilder.AddEstimator(new KeyToValueEstimator(mlContext, "PredictedLabel"));

            // STEP 4: Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
            // in order to evaluate and get the model's accuracy metrics
            Console.WriteLine("=============== Cross-validating to get model's accuracy metrics ===============");
            var crossValResults = modelBuilder.CrossValidateAndEvaluateMulticlassClassificationModel(trainingDataView, 6, "Label");
            ConsoleHelper.PrintMulticlassClassificationFoldsAverageMetrics("SdcaMultiClassTrainer", crossValResults);

            // STEP 5: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            modelBuilder.Train(trainingDataView);

            // STEP 6: Save/persist the trained model to a .ZIP file
            Console.WriteLine("=============== Saving the model to a file ===============");
            modelBuilder.SaveModelAsFile(ModelPath);

            // (OPTIONAL) Try/test a single prediction by loding the model from the file, first.
            GitHubIssue issue = new GitHubIssue() { ID = "Any-ID", Title = "Entity Framework crashes", Description = "When connecting to the database, EF is crashing" };
            var modelScorer = new ModelScorer<GitHubIssue, GitHubIssuePrediction>(mlContext);
            modelScorer.LoadModelFromZipFile(ModelPath);
            var prediction = modelScorer.PredictSingle(issue);
            Console.WriteLine($"=============== Single Prediction - Result: {prediction.Area} ===============");
            //

            Common.ConsoleHelper.ConsoleWriteHeader("Training process finalized");

        }

        private static void TestSingleLabelPrediction(string modelFilePathName)
        {
            var labeler = new Labeler(modelPath: ModelFilePathName);
            labeler.TestPredictionForSingleIssue();
        }

        private static async Task PredictLabelsAndUpdateGitHub(string ModelPath)
        {
            var token = Configuration["GitHubToken"];
            var repoOwner = Configuration["GitHubRepoOwner"]; //IMPORTANT: This can be a GitHub User or a GitHub Organization
            var repoName = Configuration["GitHubRepoName"];

            if (string.IsNullOrEmpty(token) ||
                string.IsNullOrEmpty(repoOwner) ||
                string.IsNullOrEmpty(repoName))
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