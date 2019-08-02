using CommonHelpers;
using eShopDashboard.Forecast;
using eShopDashboard.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using Microsoft.ML.Transforms.TimeSeries;
using Microsoft.ML;
using System.Linq;
using System.IO;
using Microsoft.IdentityModel.Protocols;
using System;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/producttimeseriesforecast")]
    public class ProductTimeSeriesForecastController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly MLContext mlContext = new MLContext(seed: 1);
        private readonly string ModelPath = "Forecast/ModelFiles/product988_month_timeSeriesSSA.zip";

        public ProductTimeSeriesForecastController(IOptionsSnapshot<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        [HttpGet]
        [Route("product/{productId}/unittimeseriesestimation")]
        public IActionResult GetProductUnitDemandEstimation(float productId,
            [FromQuery]int year, [FromQuery]int month,
            [FromQuery]float units, [FromQuery]float avg,
            [FromQuery]int count, [FromQuery]float max,
            [FromQuery]float min, [FromQuery]float prev)
        {

            // As the time series transformer is stateful, we're not using the prediction engine pool
            ITransformer forecaster;
            using (var file = System.IO.File.OpenRead(ModelPath))
            {
                forecaster = mlContext.Model.Load(file, out DataViewSchema schema);
            }

            // We must create a new prediction engine from the persisted model.
            TimeSeriesPredictionEngine<ProductData, ProductUnitTimeSeriesPrediction> forecastEngine = forecaster.CreateTimeSeriesEngine<ProductData, ProductUnitTimeSeriesPrediction>(mlContext);

            //Predict
            var nextMonthUnitDemandEstimation = forecastEngine.Predict();

            return Ok(nextMonthUnitDemandEstimation.ForecastedProductUnits.First());
        }
    }
}