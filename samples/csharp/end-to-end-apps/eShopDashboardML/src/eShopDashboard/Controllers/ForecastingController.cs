using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopDashboard.Forecast;
using eShopDashboard.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/forecasting")]
    public class ForecastingController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly IProductSales productSales;
        private readonly ICountrySales countrySales;

        public ForecastingController(IOptionsSnapshot<AppSettings> appSettings, IProductSales productSales, ICountrySales countrySales)
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
            [FromQuery]float min, [FromQuery]float prev)
        {
            // next,productId,year,month,units,avg,count,max,min,prev
            var nextMonthUnitDemandEstimation = productSales.Predict($"{appSettings.ForecastModelsPath}/product_month_fastTreeTweedie.zip", productId, year, month, units, avg, count, max, min, prev);

            return Ok(nextMonthUnitDemandEstimation.Score);
        }

        [HttpGet]
        [Route("country/{country}/unitdemandestimation")]
        public IActionResult GetCountrySalesForecast(string country,
            [FromQuery]int year,
            [FromQuery]int month, [FromQuery]float med,
            [FromQuery]float max, [FromQuery]float min,
            [FromQuery]float prev, [FromQuery]int count,
            [FromQuery]float sales, [FromQuery]float std)
        {
            // next,country,year,month,max,min,std,count,sales,med,prev
            var nextMonthSalesForecast = countrySales.Predict($"{appSettings.ForecastModelsPath}/country_month_fastTreeTweedie.zip", country, year, month, max, min, std, count, sales, med, prev);

            return Ok(nextMonthSalesForecast.Score);
        }
    }
}
