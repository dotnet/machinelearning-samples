using eShopForecast;
using eShopDashboard.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/countrysalesforecast")]
    public class CountrySalesForecastController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly PredictionEnginePool<CountryData, CountrySalesPrediction> countrySalesModel;
        private readonly ILogger<CountrySalesForecastController> logger;

        public CountrySalesForecastController(IOptionsSnapshot<AppSettings> appSettings,
                                              PredictionEnginePool<CountryData, CountrySalesPrediction> countrySalesModel,                                             
                                              ILogger<CountrySalesForecastController> logger)
        {
            this.appSettings = appSettings.Value;

            // Get injected Country Sales Model for scoring
            this.countrySalesModel = countrySalesModel;

            this.logger = logger;
        }

        [HttpGet]
        [Route("country/{country}/salesforecast")]
        public IActionResult GetCountrySalesForecast(string country,
                                                    [FromQuery]int year,
                                                    [FromQuery]int month, [FromQuery]float med,
                                                    [FromQuery]float max, [FromQuery]float min,
                                                    [FromQuery]float prev, [FromQuery]int count,
                                                    [FromQuery]float sales, [FromQuery]float std)
        {
            // Build country sample
            var countrySample = new CountryData()
            {
                country = country,
                year = year,
                month = month,
                max = max,
                min = min,
                std = std,
                count = count,
                sales = sales,
                med = med,
                prev = prev
            };

            this.logger.LogInformation($"Start predicting");
            //Measure execution time
            var watch = System.Diagnostics.Stopwatch.StartNew();

            CountrySalesPrediction nextMonthSalesForecast = null;

            //Predict
            nextMonthSalesForecast = this.countrySalesModel.Predict(countrySample);

            //Stop measuring time
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;
            this.logger.LogInformation($"Prediction processed in {elapsedMs} miliseconds");

            return Ok(nextMonthSalesForecast.Score);
        }
    }
}
