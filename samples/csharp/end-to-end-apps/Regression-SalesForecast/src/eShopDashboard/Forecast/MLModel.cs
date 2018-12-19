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
        private readonly int _minPredictionEngineObjectsInPool;
        private readonly int _maxPredictionEngineObjectsInPool;

        //Constructor with modelFilePathName to load
        public MLModel(MLContext mlContext, string modelFilePathName, int minPredictionEngineObjectsInPool = 10, int maxPredictionEngineObjectsInPool = 1000)
        {
            _mlContext = mlContext;

            //Load the ProductSalesForecast model from the .ZIP file
            using (var fileStream = File.OpenRead(modelFilePathName))
            {
                _model = mlContext.Model.Load(fileStream);
            }

            _minPredictionEngineObjectsInPool = minPredictionEngineObjectsInPool;
            _maxPredictionEngineObjectsInPool = maxPredictionEngineObjectsInPool;

            //Create PredictionEngine Object Pool
            _predictionEnginePool = CreatePredictionEngineObjectPool();
        }

        //Constructor with ITransformer model already created
        public MLModel(MLContext mlContext, ITransformer model, int minPredictionEngineObjectsInPool = 10, int maxPredictionEngineObjectsInPool = 1000)
        {
            _mlContext = mlContext;
            _model = model;
            _minPredictionEngineObjectsInPool = minPredictionEngineObjectsInPool;
            _maxPredictionEngineObjectsInPool = maxPredictionEngineObjectsInPool;

            //Create PredictionEngine Object Pool
            _predictionEnginePool = CreatePredictionEngineObjectPool();
        }

        private ObjectPool<PredictionFunction<TData, TPrediction>> CreatePredictionEngineObjectPool()
        {
            return new ObjectPool<PredictionFunction<TData, TPrediction>>(() => _model.MakePredictionFunction<TData, TPrediction>(_mlContext), 
                                                                          _minPredictionEngineObjectsInPool, 
                                                                          _maxPredictionEngineObjectsInPool);
        }

        public TPrediction Predict(TData dataSample)
        {
            //Get PredictionEngine object from the Object Pool
            PredictionFunction<TData, TPrediction> predictionEngine = _predictionEnginePool.GetObject();

            //Predict
            TPrediction prediction = predictionEngine.Predict(dataSample);

            //Release used PredictionEngine object into the Object Pool
            _predictionEnginePool.PutObject(predictionEngine);

            return prediction;
        }

    }
}
