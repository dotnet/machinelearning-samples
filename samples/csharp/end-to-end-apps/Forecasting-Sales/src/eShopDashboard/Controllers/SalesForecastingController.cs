using Microsoft.eShopOnContainers.Services.AI.SalesForecasting.MLNet.API.Forecasting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.MLNet.API.Controllers
{
    [Route("api/v1/ForecastingAI")]
    public class SalesForecastingController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly IProductSales productSales;
        private readonly ICountrySales countrySales;

        public SalesForecastingController(IOptionsSnapshot<AppSettings> appSettings, IProductSales productSales, ICountrySales countrySales)
        {
            this.appSettings = appSettings.Value;
            this.productSales = productSales;
            this.countrySales = countrySales;
        }

        [HttpGet]
        [Route("product/{productId}/unitdemandestimation")]
        public IActionResult GetProductUnitDemandEstimation(string productId, 
            [FromQuery]int year, [FromQuery]int month,
            [FromQuery]float units, [FromQuery]float avg,
            [FromQuery]int count, [FromQuery]float max,
            [FromQuery]float min, [FromQuery]float prev,
            [FromQuery]float price, [FromQuery]string color,
            [FromQuery]string size, [FromQuery]string shape,
            [FromQuery]string agram, [FromQuery]string bgram,
            [FromQuery]string ygram, [FromQuery]string zgram)
        {
            var nextMonthUnitDemandEstimation = productSales.Predict($"{appSettings.AIModelsPath}/product_month_fastTreeTweedle.zip", productId, year, month, units, avg, count, max, min, prev, price, color, size, shape, agram, bgram, ygram, zgram);

            return Ok(nextMonthUnitDemandEstimation.Score);
        }

        [HttpGet]
        [Route("country/{country}/salesforecast")]
        public IActionResult GetCountrySalesForecast(string country,
            [FromQuery]int year,
            [FromQuery]int month, [FromQuery]float avg, 
            [FromQuery]float max, [FromQuery]float min,
            [FromQuery]float p_max, [FromQuery]float p_min,
            [FromQuery]float p_med, 
            [FromQuery]float prev, [FromQuery]int count,
            [FromQuery]float sales, [FromQuery]float std)
        {
            var nextMonthSalesForecast = countrySales.Predict($"{appSettings.AIModelsPath}/country_month_fastTreeTweedle.zip", country, year, month, sales, avg, count, max, min, p_max, p_med, p_min, std, prev);

            return Ok(nextMonthSalesForecast.Score);
        }
    }
}
