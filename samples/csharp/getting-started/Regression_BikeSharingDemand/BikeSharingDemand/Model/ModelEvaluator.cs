using BikeSharingDemand.BikeSharingDemandData;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Learners;
using System;

namespace BikeSharingDemand.Model
{
    public sealed class ModelEvaluator<TPredictionTransformer> where TPredictionTransformer : class, ITransformer
    {
        /// <summary>
        /// Ussing passed testing/evaluation data and model, it calculates model's accuracy.
        /// </summary>
        /// <returns>Accuracy metrics of the model.</returns>
        ///      
        public RegressionEvaluator.Result Evaluate(string testDataLocation, TransformerChain<TPredictionTransformer> model)
        {
            var mlcontext = new LocalEnvironment();

            //Create TextLoader with schema related to columns in the TESTING/EVALUATION data file
            TextLoader textLoader = new BikeSharingTextLoaderFactory().CreateTextLoader(mlcontext);
            
            //Load evaluation/test data
            IDataView testDataView = textLoader.Read(new MultiFileSource(testDataLocation));

            Console.WriteLine("=============== Evaluating Model's accuracy with Test data===============");
            var predictions = model.Transform(testDataView);

            var regressionCtx = new RegressionContext(mlcontext);
            var metrics = regressionCtx.Evaluate(predictions, "Count", "Score");

            return metrics;
        }

        public void PrintRegressionMetrics(string name, RegressionEvaluator.Result metrics)
        {
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Metrics for {name}          ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       LossFn: {metrics.LossFn:0.##}");
            Console.WriteLine($"*       R2 Score: {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Absolute loss: {metrics.L1:#.##}");
            Console.WriteLine($"*       Squared loss: {metrics.L2:#.##}");
            Console.WriteLine($"*       RMS loss: {metrics.Rms:#.##}");
            Console.WriteLine($"*************************************************");
        }
    }
}
