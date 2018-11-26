
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using System.IO;

namespace eShopDashboard.Forecast
{
    /// <summary>
    /// This is the output of the scored model, the prediction.
    /// </summary>
    public class ProductUnitPrediction
    {
        // Below columns are produced by the model's predictor.
        public float Score;
    }

}
