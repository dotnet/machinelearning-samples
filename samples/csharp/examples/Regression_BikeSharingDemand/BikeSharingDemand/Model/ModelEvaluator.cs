using BikeSharingDemand.BikeSharingDemandData;

using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Models;
using Microsoft.ML.Legacy.Data;


namespace BikeSharingDemand.Model
{
    public class ModelEvaluator
    {
        /// <summary>
        /// Ussing passed testing data and model, it calculates model's accuracy.
        /// </summary>
        /// <returns>Accuracy of the model.</returns>
        public RegressionMetrics Evaluate(PredictionModel<BikeSharingDemandSample, BikeSharingDemandPrediction> model, string testDataLocation)
        {
            var testData = new TextLoader(testDataLocation).CreateFrom<BikeSharingDemandSample>(useHeader: true, separator: ',');
            var metrics = new RegressionEvaluator().Evaluate(model, testData);
            return metrics;
        }
    }
}
