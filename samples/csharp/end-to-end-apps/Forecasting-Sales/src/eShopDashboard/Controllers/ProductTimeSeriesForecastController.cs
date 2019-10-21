using eShopForecast;
using eShopDashboard.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.ML.Transforms.TimeSeries;
using Microsoft.ML;
using System.Linq;
using eShopDashboard.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/producttimeseriesforecast")]
    public class ProductTimeSeriesForecastController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly MLContext mlContext = new MLContext();
        private readonly IOrderingQueries _queries;

        public ProductTimeSeriesForecastController(IOptionsSnapshot<AppSettings> appSettings, IOrderingQueries queries)
        {
            //Note: The ProductRegressionForecastController uses the PredictionEnginePool for loading and caching the model.
            //Notice that for Time Series, we do NOT use the PredictionEnginePool.  Instead, Time Series model is stateful because
            //you must regularly update the state of the model with new observed data points and as a result, should never be cached.
            //Refer to the sample's ReadMe for further details on this.

            this.appSettings = appSettings.Value;
            this._queries = queries;
        }

        [HttpGet]
        [Route("product/{productId}/unittimeseriesestimation")]
        public async Task<IActionResult> GetProductUnitDemandEstimation(string productId)
        {
            // Get product history for the selected product
            var productHistory = await _queries.GetProductDataAsync(productId);
            var productDataView = mlContext.Data.LoadFromEnumerable(productHistory);

            // Create and add the forecast estimator to the pipeline.
            IEstimator<ITransformer> forecastEstimator = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(ProductUnitTimeSeriesPrediction.ForecastedProductUnits),
                inputColumnName: nameof(ProductData.units), // This is the column being forecasted.
                windowSize: 12, // Window size is set to the time period represented in the product data cycle; our product cycle is based on 12 months, so this is set to a factor of 12, e.g. 3.
                seriesLength: productHistory.Count(), // This parameter specifies the number of data points that are used when performing a forecast.
                trainSize: productHistory.Count(), // This parameter specifies the total number of data points in the input time series, starting from the beginning.
                horizon: 1, // Indicates the number of values to forecast; 1 indicates that the next month of product units will be forecasted.
                confidenceLevel: 0.95f, // Indicates the likelihood the real observed value will fall within the specified interval bounds.
                confidenceLowerBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceLowerBound), //This is the name of the column that will be used to store the lower interval bound for each forecasted value.
                confidenceUpperBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceUpperBound)); //This is the name of the column that will be used to store the upper interval bound for each forecasted value.

            // Train the forecasting model for the specified product's data series.
            ITransformer forecastTransformer = forecastEstimator.Fit(productDataView);

            // Create the forecast engine used for creating predictions.
            TimeSeriesPredictionEngine<ProductData, ProductUnitTimeSeriesPrediction> forecastEngine = forecastTransformer.CreateTimeSeriesEngine<ProductData, ProductUnitTimeSeriesPrediction>(mlContext);

            // Predict.
            var nextMonthUnitDemandEstimation = forecastEngine.Predict();

            return Ok(nextMonthUnitDemandEstimation.ForecastedProductUnits.First());
        }
    }
}