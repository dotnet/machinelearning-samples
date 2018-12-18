using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using System.IO;
using Microsoft.ML;
using Microsoft.Extensions.Configuration;

namespace eShopDashboard.Forecast
{
    public class MLModel<TData, TPrediction> 
                        where TData : class
                        where TPrediction : class, new()
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;
        private readonly ObjectPool<PredictionFunction<TData, TPrediction>> _predictionEnginePool;

        //MLContext is injected through DI/IoC
        public MLModel(MLContext mlContext, IConfiguration configuration)
        {
            _mlContext = mlContext;
            string modelFolder = configuration["ForecastModelsPath"];

            //Load the ProductSalesForecast model from the .ZIP file
            using (var fileStream = File.OpenRead($"{modelFolder}/country_month_fastTreeTweedie.zip"))
            {
                _model = mlContext.Model.Load(fileStream);
            }

            //Create PredictionEngine Object Pool
            _predictionEnginePool = new ObjectPool<PredictionFunction<TData, TPrediction>>(() => _model.MakePredictionFunction<TData, TPrediction>(_mlContext));
        }

        /// <summary>
        /// This function creates a prediction function from the loaded model.
        /// </summary>
        public PredictionFunction<CountryData, CountrySalesPrediction> CreatePredictionFunction()
        {
            return _model.MakePredictionFunction<CountryData, CountrySalesPrediction>(_mlContext);
        }

    }
}
