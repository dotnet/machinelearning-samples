using System;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Api;

using BikeSharingDemand.DataStructures;

namespace BikeSharingDemand
{
    internal static class Program
    {
        private static string ModelsLocation = @"../../../../MLModels";

        private static string DatasetsLocation = @"../../../../Data";
        private static string TrainingDataLocation = $"{DatasetsLocation}/hour_train.csv";
        private static string TestDataLocation = $"{DatasetsLocation}/hour_test.csv";
        
        static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 0);

            // 1. Common data loading
            DataLoader dataLoader = new DataLoader(mlContext);
            var trainingDataView = dataLoader.GetDataView(TrainingDataLocation);
            var testDataView = dataLoader.GetDataView(TestDataLocation);

            // 2. Common data pre-process with pipeline data transformations
            var dataProcessor = new DataProcessor(mlContext);
            var dataProcessPipeline = dataProcessor.DataProcessPipeline;

            // (Optional) Peek data in training DataView after applying the ProcessPipeline's transformations  
            Common.ConsoleHelper.PeekDataViewInConsole<DemandObservation>(mlContext, trainingDataView, dataProcessPipeline, 10);
            Common.ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 10);

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
            foreach (var learner in regressionLearners)
            {
                Console.WriteLine("================== Training model ==================");
                var modelBuilder = new Common.ModelBuilder<DemandObservation,DemandPrediction>(mlContext, dataProcessPipeline);
                modelBuilder.AddTrainer(learner.value);
                var trainedModel = modelBuilder.Train(trainingDataView);

                Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
                var metrics = modelBuilder.EvaluateRegressionModel(testDataView, "Count", "Score");
                Common.ConsoleHelper.PrintRegressionMetrics(learner.name, metrics);

                //Save the model file that can be used by any application
                modelBuilder.SaveModelAsFile($"{ModelsLocation}/{learner.name}Model.zip");
            }

            // 4. Try/test Predictions with the created models
            // The following test predictions could be implemented/deployed in a different application (production apps)
            // that's why it is seggregated from the previous loop
            // For each trained model, test 10 predictions           
            foreach (var learner in regressionLearners)
            {
                //Load current model
                var modelScorer = new Common.ModelScorer<DemandObservation, DemandPrediction>(mlContext);
                modelScorer.LoadModelFromZipFile($"{ModelsLocation}/{learner.name}Model.zip");

                Console.WriteLine($"================== Visualize/test 10 predictions for model {learner.name}Model.zip ==================");
                //Visualize 10 tests comparing prediction with actual/observed values from the test dataset
                ModelScoringTester.VisualizeSomePredictions(mlContext ,learner.name, TestDataLocation, modelScorer, 10);
            }

            Common.ConsoleHelper.ConsolePressAnyKey();

        }
    }
}
