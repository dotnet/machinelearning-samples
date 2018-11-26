
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using System.IO;

namespace eShopDashboard.Forecast
{
    public class ProductSalesModel
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;

        //MLContext is injected through DI/IoC
        public ProductSalesModel(MLContext mlContext, IConfiguration configuration)
        {
            _mlContext = mlContext;
            string modelFolder = configuration["ForecastModelsPath"];

            //Load the ProductSalesForecast model from the .ZIP file
            using (var fileStream = File.OpenRead($"{modelFolder}/product_month_fastTreeTweedie.zip"))
            {
                _model = mlContext.Model.Load(fileStream);
            }         
        }

        /// <summary>
        /// This function creates a prediction engine from the loaded model.
        /// </summary>
        public PredictionFunction<ProductData, ProductUnitPrediction> CreatePredictionFunction()
        {
            return _model.MakePredictionFunction<ProductData, ProductUnitPrediction>(_mlContext);
        }
    }
}
