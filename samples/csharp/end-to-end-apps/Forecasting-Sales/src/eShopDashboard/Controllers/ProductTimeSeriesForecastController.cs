using CommonHelpers;
using eShopDashboard.Forecast;
using eShopDashboard.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/producttimeseriesforecast")]
    public class ProductTimeSeriesForecastController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly PredictionEnginePool<ProductData, ProductUnitTimeSeriesPrediction> productSalesModel;

        public ProductTimeSeriesForecastController(IOptionsSnapshot<AppSettings> appSettings,
                                               PredictionEnginePool<ProductData, ProductUnitTimeSeriesPrediction> productSalesModel)
        {
            this.appSettings = appSettings.Value;

            // Get injected Product Sales Model for scoring
            this.productSalesModel = productSalesModel;
        }

        [HttpGet]
        [Route("product/{productId}/unittimeseriesestimation")]
        public IActionResult GetProductUnitDemandEstimation(string productId,
            [FromQuery]int year, [FromQuery]int month,
            [FromQuery]float units, [FromQuery]float avg,
            [FromQuery]int count, [FromQuery]float max,
            [FromQuery]float min, [FromQuery]float prev)
        {
            // Build product sample
            var inputExample = new ProductData(productId, year, month, units, avg, count, max, min, prev);

            ProductUnitTimeSeriesPrediction nextMonthUnitDemandEstimation = null;

            //Predict
            nextMonthUnitDemandEstimation = this.productSalesModel.Predict(inputExample);

            return Ok(nextMonthUnitDemandEstimation.ForecastedProductUnits);
        }
    }
}