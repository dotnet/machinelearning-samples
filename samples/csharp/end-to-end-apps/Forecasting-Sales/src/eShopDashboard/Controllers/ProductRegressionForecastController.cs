using eShopForecast;
using eShopDashboard.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/productdemandforecast")] 
    public class ProductRegressionForecastController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly PredictionEnginePool<ProductData, ProductUnitRegressionPrediction> productSalesModel;

        public ProductRegressionForecastController(IOptionsSnapshot<AppSettings> appSettings,
                                               PredictionEnginePool<ProductData, ProductUnitRegressionPrediction> productSalesModel)
        {
            this.appSettings = appSettings.Value;

            // Get injected Product Sales Model for scoring
            this.productSalesModel = productSalesModel;
        }

        [HttpGet]
        [Route("product/{productId}/unitdemandestimation")]
        public IActionResult GetProductUnitDemandEstimation(float productId,
            [FromQuery]int year, [FromQuery]int month,
            [FromQuery]float units, [FromQuery]float avg,
            [FromQuery]int count, [FromQuery]float max,
            [FromQuery]float min, [FromQuery]float prev)
        {
            // Build product sample
            var inputExample = new ProductData()
            {
                productId = productId,
                year = year,
                month = month,
                units = units,
                avg = avg,
                count = count,
                max = max,
                min = min,
                prev = prev
            };
          
            ProductUnitRegressionPrediction nextMonthUnitDemandEstimation = null;

            //Predict
            nextMonthUnitDemandEstimation = this.productSalesModel.Predict(inputExample);

            return Ok(nextMonthUnitDemandEstimation.Score);
        }
    }
}
