using System;
using System.Linq;
using System.Runtime.InteropServices;
using BikeSharingDemand.BikeSharingDemandData;
using BikeSharingDemand.Helpers;
using BikeSharingDemand.Model;
using Microsoft.ML;
using Microsoft.ML.Models;
using Microsoft.ML.Trainers;

namespace BikeSharingDemand
{
    class Program
    {
        static void Main(string[] args)
        {
            var trainingDataLocation = @"Data/hour_train.csv";
            var testDataLocation = @"Data/hour_test.csv";

            var modelEvaluator = new ModelEvaluator();

            var fastTreeModel = new ModelBuilder(trainingDataLocation, new FastTreeRegressor()).BuildAndTrain();
            var fastTreeMetrics = modelEvaluator.Evaluate(fastTreeModel, testDataLocation);
            PrintMetrics("Fast Tree", fastTreeMetrics);

            var fastForestModel = new ModelBuilder(trainingDataLocation, new FastForestRegressor()).BuildAndTrain();
            var fastForestMetrics = modelEvaluator.Evaluate(fastForestModel, testDataLocation);
            PrintMetrics("Fast Forest", fastForestMetrics);

            var poissonModel = new ModelBuilder(trainingDataLocation, new PoissonRegressor()).BuildAndTrain();
            var poissonMetrics = modelEvaluator.Evaluate(poissonModel, testDataLocation);
            PrintMetrics("Poisson", poissonMetrics);

            var gradientDescentModel = new ModelBuilder(trainingDataLocation, new OnlineGradientDescentRegressor()).BuildAndTrain();
            var gradientDescentMetrics = modelEvaluator.Evaluate(gradientDescentModel, testDataLocation);
            PrintMetrics("Online Gradient Descent", gradientDescentMetrics);

            var fastTreeTweedieModel = new ModelBuilder(trainingDataLocation, new FastTreeTweedieRegressor()).BuildAndTrain();
            var fastTreeTweedieMetrics = modelEvaluator.Evaluate(fastTreeTweedieModel, testDataLocation);
            PrintMetrics("Fast Tree Tweedie", fastTreeTweedieMetrics);

            var additiveModel = new ModelBuilder(trainingDataLocation, new GeneralizedAdditiveModelRegressor()).BuildAndTrain();
            var additiveMetrics = modelEvaluator.Evaluate(additiveModel, testDataLocation);
            PrintMetrics("Generalized Additive Model", additiveMetrics);

            var stohasticDualCorordinateAscentModel = new ModelBuilder(trainingDataLocation, new StochasticDualCoordinateAscentRegressor()).BuildAndTrain();
            var stohasticDualCorordinateAscentMetrics = modelEvaluator.Evaluate(stohasticDualCorordinateAscentModel, testDataLocation);
            PrintMetrics("Stochastic Dual Coordinate Ascent", stohasticDualCorordinateAscentMetrics);

            VisualizeTenPredictionsForTheModel(fastTreeTweedieModel, testDataLocation);
            fastTreeTweedieModel.WriteAsync(@".\Model.zip");
            
            Console.ReadLine();
        }

        private static void PrintMetrics(string name, RegressionMetrics metrics)
        {
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Metrics for {name}          ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       R2 Score: {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Absolute loss: {metrics.L1:#.##}");
            Console.WriteLine($"*       Squared loss: {metrics.L2:#.##}");
            Console.WriteLine($"*       RMS loss: {metrics.Rms:#.##}");
            Console.WriteLine($"*************************************************");
        }

        private static void VisualizeTenPredictionsForTheModel(
            PredictionModel<BikeSharingDemandSample, BikeSharingDemandPrediction> model,
            string testDataLocation)
        {
            var testData = new BikeSharingDemandsCsvReader().GetDataFromCsv(testDataLocation).ToList();
            for (int i = 0; i < 10; i++)
            {
                var prediction = model.Predict(testData[i]);
                Console.WriteLine($"-------------------------------------------------");
                Console.WriteLine($"Predicted : {prediction.PredictedCount}");
                Console.WriteLine($"Actual:    {testData[i].Count}");
                Console.WriteLine($"-------------------------------------------------");
            }
        }
    }
}
