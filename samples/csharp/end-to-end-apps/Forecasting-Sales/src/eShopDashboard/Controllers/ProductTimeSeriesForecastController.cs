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
            this.appSettings = appSettings.Value;
            this._queries = queries;
        }

        [HttpGet]
        [Route("product/{productId}/unittimeseriesestimation")]
        public async Task<IActionResult> GetProductUnitDemandEstimation(string productId)
        {
            // Get product history
            var productHistory = await _queries.GetProductDataAsync(productId);

            // Supplement the history with synthetic data
            var supplementedProductHistory = TimeSeriesDataGenerator.SupplementData(mlContext, productHistory);
            var supplementedProductHistoryLength = supplementedProductHistory.Count(); // 36
            var supplementedProductDataView = mlContext.Data.LoadFromEnumerable(supplementedProductHistory);

            // Create and add the forecast estimator to the pipeline.
            IEstimator<ITransformer> forecastEstimator = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(ProductUnitTimeSeriesPrediction.ForecastedProductUnits),
                inputColumnName: nameof(ProductData.units), // This is the column being forecasted.
                windowSize: 12, // Window size is set to the time period represented in the product data cycle; our product cycle is based on 12 months, so this is set to a factor of 12, e.g. 3.
                seriesLength: supplementedProductHistoryLength, // TODO: Need clarification on what this should be set to; assuming product series length for now.
                trainSize: supplementedProductHistoryLength, // TODO: Need clarification on what this should be set to; assuming product series length for now.
                horizon: 1, // Indicates the number of values to forecast; 1 indicates that the next month of product units will be forecasted.
                confidenceLevel: 0.95f, // TODO: Is this the same as prediction interval, where this indicates that we are 95% confidence that the forecasted value will fall within the interval range?
                confidenceLowerBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceLowerBound), // TODO: See above comment.
                confidenceUpperBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceUpperBound)); // TODO: See above comment.

            // Train the forecasting model for the specified product's data series.
            ITransformer forecastTransformer = forecastEstimator.Fit(supplementedProductDataView);

            // Create the forecast engine used for creating predictions.
            TimeSeriesPredictionEngine<ProductData, ProductUnitTimeSeriesPrediction> forecastEngine = forecastTransformer.CreateTimeSeriesEngine<ProductData, ProductUnitTimeSeriesPrediction>(mlContext);

            // Predict
            var nextMonthUnitDemandEstimation = forecastEngine.Predict();

            return Ok(nextMonthUnitDemandEstimation.ForecastedProductUnits.First());
        }
    }
}