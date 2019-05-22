﻿using CommonHelpers;
using eShopDashboard.Forecast;
using eShopDashboard.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/productdemandforecast")] 
    public class ProductDemandForecastController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly PredictionEnginePool<ProductData, ProductUnitPrediction> productSalesModel;

        public ProductDemandForecastController(IOptionsSnapshot<AppSettings> appSettings,
                                               PredictionEnginePool<ProductData, ProductUnitPrediction> productSalesModel)
        {
            this.appSettings = appSettings.Value;

            // Get injected Product Sales Model for scoring
            this.productSalesModel = productSalesModel;
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

            //Predict
            nextMonthUnitDemandEstimation = this.productSalesModel.Predict(inputExample);

            return Ok(nextMonthUnitDemandEstimation.Score);
        }
    }
}
