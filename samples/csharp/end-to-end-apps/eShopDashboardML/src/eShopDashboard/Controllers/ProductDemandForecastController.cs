using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopDashboard.Forecast;
using eShopDashboard.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.ML.Runtime.Data;
using Serilog;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/productdemandforecast")] 
    public class ProductDemandForecastController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly PredictionFunction<ProductData, ProductUnitPrediction> productSalesPredFunction;

        public ProductDemandForecastController(IOptionsSnapshot<AppSettings> appSettings, 
                                     PredictionFunction<ProductData, ProductUnitPrediction> productSalesPredFunction
                                    )
        {
            this.appSettings = appSettings.Value;

            // Get injected Product Sales Prediction function
            this.productSalesPredFunction = productSalesPredFunction;
        }

        [HttpGet]
        [Route("product/{productId}/unitdemandestimation")]
        public IActionResult GetProductUnitDemandEstimation(string productId,
            [FromQuery]int year, [FromQuery]int month,
            [FromQuery]float units, [FromQuery]float avg,
            [FromQuery]int count, [FromQuery]float max,
            [FromQuery]float min, [FromQuery]float prev)
        {
            // Build product sample
            var inputExample = new ProductData(productId, year, month, units, avg, count, max, min, prev);

            
            ProductUnitPrediction nextMonthUnitDemandEstimation = null;

            //Set the critical section if using Singleton for the PredictionFunction object
            //               
            //lock(this.productSalesPredFunction)
            //{
                // Returns prediction
                nextMonthUnitDemandEstimation = this.productSalesPredFunction.Predict(inputExample);
            //}
            //
            // Note that if using Scoped instead of singleton in DI/IoC you can remove the critical section
            // It depends if you want better performance in single Http calls (using singleton) 
            // versus better scalability ann global performance if you have many Http requests/threads 
            // since the critical section is a bottleneck reducing the execution to one thread for that particular Predict() mathod call
            //

            return Ok(nextMonthUnitDemandEstimation.Score);
        }
    }
}
