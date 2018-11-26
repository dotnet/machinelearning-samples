using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopDashboard.Forecast;
using eShopDashboard.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML.Runtime.Data;
using Serilog;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/countrysalesforecast")]
    public class CountrySalesForecastController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly PredictionFunction<CountryData, CountrySalesPrediction> countrySalesPredFunction;
        private readonly ILogger<CountrySalesForecastController> logger;

        public CountrySalesForecastController(IOptionsSnapshot<AppSettings> appSettings,
                                              PredictionFunction<CountryData, CountrySalesPrediction> countrySalesPredFunction,
                                              ILogger<CountrySalesForecastController> logger
                                             )
        {
            this.appSettings = appSettings.Value;

            // Get injected Country Sales Prediction function
            this.countrySalesPredFunction = countrySalesPredFunction;

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
            var countrySample = new CountryData(country, year, month, max, min, std, count, sales, med, prev);

            this.logger.LogInformation($"Start predicting");
            //Measure execution time
            var watch = System.Diagnostics.Stopwatch.StartNew();

            CountrySalesPrediction nextMonthSalesForecast = null;

            //Set the critical section if using Singleton for the PredictionFunction object
            //               
            //lock(this.countrySalesPredFunction)
            //{
                //Predict action (Measure Prediction function Singleton vs. Scoped)
                nextMonthSalesForecast = this.countrySalesPredFunction.Predict(countrySample);
            //}
            //
            // Note that if using Scoped instead of singleton in DI/IoC you can remove the critical section
            // It depends if you want better performance in single Http calls (using singleton) 
            // versus better scalability ann global performance if you have many Http requests/threads 
            // since the critical section is a bottleneck reducing the execution to one thread for that particular Predict() mathod call
            //

            //Stop measuring time
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;
            this.logger.LogInformation($"Prediction processed in {elapsedMs} miliseconds");

            return Ok(nextMonthSalesForecast.Score);
        }
    }
}
