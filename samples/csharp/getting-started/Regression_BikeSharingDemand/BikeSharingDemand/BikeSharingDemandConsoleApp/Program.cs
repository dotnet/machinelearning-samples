using System;
using Microsoft.ML;
using System.IO;
using Microsoft.ML.Data;

using BikeSharingDemand.DataStructures;
using Common;
using Microsoft.Data.DataView;

namespace BikeSharingDemand
{
    internal static class Program
    {
        private static string ModelsLocation = @"../../../../MLModels";

        private static string DatasetsLocation = @"../../../../Data";
        private static string TrainingDataRelativePath = $"{DatasetsLocation}/hour_train.csv";
        private static string TestDataRelativePath = $"{DatasetsLocation}/hour_test.csv";

        private static string TrainingDataLocation = GetDataSetAbsolutePath(TrainingDataRelativePath);
        private static string TestDataLocation = GetDataSetAbsolutePath(TestDataRelativePath);
        
        static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 0);

            // 1. Common data loading configuration
            var trainingDataView = mlContext.Data.ReadFromTextFile<DemandObservation>(path: TrainingDataLocation, hasHeader:true, separatorChar: ',');
            var testDataView = mlContext.Data.ReadFromTextFile<DemandObservation>(path: TestDataLocation, hasHeader:true, separatorChar: ',');

            // 2. Common data pre-process with pipeline data transformations

            // Concatenate all the numeric columns into a single features column
            var dataProcessPipeline = mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
                                                     nameof(DemandObservation.Season), nameof(DemandObservation.Year), nameof(DemandObservation.Month),
                                                     nameof(DemandObservation.Hour), nameof(DemandObservation.Holiday), nameof(DemandObservation.Weekday),
                                                     nameof(DemandObservation.WorkingDay), nameof(DemandObservation.Weather), nameof(DemandObservation.Temperature),
                                                     nameof(DemandObservation.NormalizedTemperature), nameof(DemandObservation.Humidity), nameof(DemandObservation.Windspeed))
                                         .AppendCacheCheckpoint(mlContext);

            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            Common.ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline, 10);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, DefaultColumnNames.Features, trainingDataView, dataProcessPipeline, 10);

            // Definition of regression trainers/algorithms to use
            //var regressionLearners = new (string name, IEstimator<ITransformer> value)[]
            (string name, IEstimator<ITransformer> value)[] regressionLearners =
            {
                ("FastTree", mlContext.Regression.Trainers.FastTree()),
                ("Poisson", mlContext.Regression.Trainers.PoissonRegression()),
                ("SDCA", mlContext.Regression.Trainers.StochasticDualCoordinateAscent()),
                ("FastTreeTweedie", mlContext.Regression.Trainers.FastTreeTweedie()),
                //Other possible learners that could be included
                //...FastForestRegressor...
                //...GeneralizedAdditiveModelRegressor...
                //...OnlineGradientDescent... (Might need to normalize the features first)
            };

            // 3. Phase for Training, Evaluation and model file persistence
            // Per each regression trainer: Train, Evaluate, and Save a different model
            foreach (var trainer in regressionLearners)
            {
                Console.WriteLine("=============== Training the current model ===============");
                var trainingPipeline = dataProcessPipeline.Append(trainer.value);
                var trainedModel = trainingPipeline.Fit(trainingDataView);

                Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
                IDataView predictions = trainedModel.Transform(testDataView);
                var metrics = mlContext.Regression.Evaluate(data:predictions, label:DefaultColumnNames.Label, score: DefaultColumnNames.Score);               
                ConsoleHelper.PrintRegressionMetrics(trainer.value.ToString(), metrics);

                //Save the model file that can be used by any application
                string modelRelativeLocation = $"{ModelsLocation}/{trainer.name}Model.zip";
                string modelPath = GetDataSetAbsolutePath(modelRelativeLocation);
                using (var fs = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                    mlContext.Model.Save(trainedModel, fs);

                Console.WriteLine("The model is saved to {0}", modelPath);
            }

            // 4. Try/test Predictions with the created models
            // The following test predictions could be implemented/deployed in a different application (production apps)
            // that's why it is seggregated from the previous loop
            // For each trained model, test 10 predictions           
            foreach (var learner in regressionLearners)
            {
                //Load current model from .ZIP file
                ITransformer trainedModel;
                string modelRelativeLocation = $"{ModelsLocation}/{learner.name}Model.zip";
                string modelPath = GetDataSetAbsolutePath(modelRelativeLocation);
                using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    trainedModel = mlContext.Model.Load(stream);
                }

                // Create prediction engine related to the loaded trained model
                var predEngine = trainedModel.CreatePredictionEngine<DemandObservation, DemandPrediction>(mlContext);

                Console.WriteLine($"================== Visualize/test 10 predictions for model {learner.name}Model.zip ==================");
                //Visualize 10 tests comparing prediction with actual/observed values from the test dataset
                ModelScoringTester.VisualizeSomePredictions(mlContext ,learner.name, TestDataLocation, predEngine, 10);
            }

            Common.ConsoleHelper.ConsolePressAnyKey();
        }

        public static string GetDataSetAbsolutePath(string relativeDatasetPath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativeDatasetPath);

            return fullPath;
        }
    }
}
