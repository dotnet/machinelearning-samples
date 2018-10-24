using BikeSharingDemand.BikeSharingDemandData;
using BikeSharingDemand.Helpers;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Learners;
using System;
using System.Linq;

namespace BikeSharingDemand.Model
{
    public sealed class ModelTester<TPredictionTransformer> where TPredictionTransformer : class, ITransformer
    {
        /// <summary>
        /// Using test data and model, it shows the predicted versus the observed data
        /// </summary>
        ///      
        public void VisualizeSomePredictions(string modelName, string testDataLocation, TransformerChain<TPredictionTransformer> model, int numberOfPredictions)
        {
            //Prediction test
            var mlcontext = new LocalEnvironment();

            // Create prediction engine 
            var engine = model.MakePredictionFunction<BikeSharingDemandSample, BikeSharingDemandPrediction>(mlcontext);

            //Make the provided number of predictions and compare with observed data from the test dataset
            var testData = new BikeSharingDemandsCsvReader().GetDataFromCsv(testDataLocation).ToList();

            Console.WriteLine($"=======================================================");
            Console.WriteLine($"=======       Tests with {modelName}       ========");

            for (int i = 0; i < numberOfPredictions; i++)
            {
                var prediction = engine.Predict(testData[i]);
       
                Console.WriteLine($"-------------------------------------------------");
                Console.WriteLine($"Predicted : {prediction.PredictedCount}");
                Console.WriteLine($"Actual:     {testData[i].Count}");
                Console.WriteLine($"-------------------------------------------------");
            }

            Console.WriteLine($"=======================================================");
            Console.WriteLine();
        }

        //public void PrintRegressionMetrics(string name, RegressionEvaluator.Result metrics)
        //{
        //    Console.WriteLine($"*************************************************");
        //    Console.WriteLine($"*       Metrics for {name}          ");
        //    Console.WriteLine($"*------------------------------------------------");
        //    Console.WriteLine($"*       LossFn: {metrics.LossFn:0.##}");
        //    Console.WriteLine($"*       R2 Score: {metrics.RSquared:0.##}");
        //    Console.WriteLine($"*       Absolute loss: {metrics.L1:#.##}");
        //    Console.WriteLine($"*       Squared loss: {metrics.L2:#.##}");
        //    Console.WriteLine($"*       RMS loss: {metrics.Rms:#.##}");
        //    Console.WriteLine($"*************************************************");
        //}
    }
}
