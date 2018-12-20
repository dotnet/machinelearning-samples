using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using System.IO;
using Microsoft.ML;
//using Microsoft.Extensions.Configuration;

namespace Common
{
    public class MLModelEngine<TData, TPrediction> 
                        where TData : class
                        where TPrediction : class, new()
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;
        private readonly ObjectPool<PredictionFunction<TData, TPrediction>> _predictionEnginePool;
        private readonly int _minPredictionEngineObjectsInPool;
        private readonly int _maxPredictionEngineObjectsInPool;

        public int CurrentPredictionEnginePoolSize
        {
            get { return _predictionEnginePool.CurrentPoolSize; }
        }

        //Constructor with modelFilePathName to load
        public MLModelEngine(MLContext mlContext, string modelFilePathName, int minPredictionEngineObjectsInPool = 5, int maxPredictionEngineObjectsInPool = 1000)
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
        public MLModelEngine(MLContext mlContext, ITransformer model, int minPredictionEngineObjectsInPool = 5, int maxPredictionEngineObjectsInPool = 1000)
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
            return new ObjectPool<PredictionFunction<TData, TPrediction>>(objectGenerator:() =>
                                                                              {
                                                                                  //Measure PredictionEngine creation
                                                                                  var watch = System.Diagnostics.Stopwatch.StartNew();

                                                                                  //Make PredictionEngine
                                                                                  var predEngine = _model.MakePredictionFunction<TData, TPrediction>(_mlContext);

                                                                                  //Stop measuring time
                                                                                  watch.Stop();
                                                                                  long elapsedMs = watch.ElapsedMilliseconds;
                                                                                  
                                                                                  return predEngine;
                                                                              }, 
                                                                          minPoolSize: _minPredictionEngineObjectsInPool,
                                                                          maxPoolSize: _maxPredictionEngineObjectsInPool);
        }

        public TPrediction Predict(TData dataSample)
        {
            //Get PredictionEngine object from the Object Pool
            PredictionFunction<TData, TPrediction> predictionEngine = _predictionEnginePool.GetObject();

            //Measure Predict() execution time
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //Predict
            TPrediction prediction = predictionEngine.Predict(dataSample);

            //Stop measuring time
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;

            //Release used PredictionEngine object into the Object Pool
            _predictionEnginePool.PutObject(predictionEngine);

            return prediction;
        }

    }
}
