using Microsoft.ML.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.Extensions.Configuration;

namespace eShopDashboard.Forecast
{
    /// <summary>
    /// This is the output of the scored model, the prediction.
    /// </summary>
    public class CountrySalesPrediction
    {
        // Below columns are produced by the model's predictor.
        public float Score;
    }

}
