using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.ML.Legacy;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML;
using Microsoft.Extensions.Configuration;

namespace eShopDashboard.Forecast
{
    public class CountrySalesModel
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;

        //MLContext is injected through DI/IoC
        public CountrySalesModel(MLContext mlContext, IConfiguration configuration)
        {
            _mlContext = mlContext;
            string modelFolder = configuration["ForecastModelsPath"];

            //Load the ProductSalesForecast model from the .ZIP file
            using (var fileStream = File.OpenRead($"{modelFolder}/country_month_fastTreeTweedie.zip"))
            {
                _model = mlContext.Model.Load(fileStream);
            }
        }

        /// <summary>
        /// This function creates a prediction function from the loaded model.
        /// </summary>
        public PredictionFunction<CountryData, CountrySalesPrediction> CreatePredictionFunction()
        {
            return _model.MakePredictionFunction<CountryData, CountrySalesPrediction>(_mlContext);
        }

    }
}
