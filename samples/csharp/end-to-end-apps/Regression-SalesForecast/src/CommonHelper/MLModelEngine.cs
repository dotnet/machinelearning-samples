using System.IO;
using Microsoft.ML;

namespace CommonHelpers
{
    public class MLModelEngine<TData, TPrediction> 
                        where TData : class
                        where TPrediction : class, new()
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;
        private readonly ObjectPool<PredictionEngine<TData, TPrediction>> _predictionEnginePool;
        private readonly int _minPredictionEngineObjectsInPool;
        private readonly int _maxPredictionEngineObjectsInPool;
        private readonly double _expirationTime;

        public int CurrentPredictionEnginePoolSize
        {
            get { return _predictionEnginePool.CurrentPoolSize; }
        }

        /// <summary>
        /// Constructor with modelFilePathName to load
        /// </summary>
        /// <param name="mlContext">MLContext to use</param>
        /// <param name="modelFilePathName">Model .ZIP file path name</param>
        /// <param name="minPredictionEngineObjectsInPool">Minimum number of PredictionEngineObjects in pool, as goal. Could be less but eventually it'll tend to that number</param>
        /// <param name="maxPredictionEngineObjectsInPool">Maximum number of PredictionEngineObjects in pool</param>
        /// <param name="expirationTime">Expiration Time (mlSecs) of PredictionEngineObject since added to the pool</param>
        public MLModelEngine(MLContext mlContext, string modelFilePathName, int minPredictionEngineObjectsInPool = 5, int maxPredictionEngineObjectsInPool = 1000, double expirationTime = 30000)
        {
            _mlContext = mlContext;

            //Load the ProductSalesForecast model from the .ZIP file
            _model = mlContext.Model.Load(modelFilePathName, out var modelInputSchema);

            _minPredictionEngineObjectsInPool = minPredictionEngineObjectsInPool;
            _maxPredictionEngineObjectsInPool = maxPredictionEngineObjectsInPool;
            _expirationTime = expirationTime;

            //Create PredictionEngine Object Pool
            _predictionEnginePool = CreatePredictionEngineObjectPool();
        }

        /// <summary>
        /// Constructor with ITransformer model already created
        /// </summary>
        /// <param name="mlContext">MLContext to use</param>
        /// <param name="model">Model/Transformer to use, already created</param>
        /// <param name="minPredictionEngineObjectsInPool">Minimum number of PredictionEngineObjects in pool, as goal. Could be less but eventually it'll tend to that number</param>
        /// <param name="maxPredictionEngineObjectsInPool">Maximum number of PredictionEngineObjects in pool</param>
        /// <param name="expirationTime">Expiration Time (mlSecs) of PredictionEngineObject since added to the pool</param>
        public MLModelEngine(MLContext mlContext, ITransformer model, int minPredictionEngineObjectsInPool = 5, int maxPredictionEngineObjectsInPool = 1000, double expirationTime = 30000)
        {
            _mlContext = mlContext;
            _model = model;
            _minPredictionEngineObjectsInPool = minPredictionEngineObjectsInPool;
            _maxPredictionEngineObjectsInPool = maxPredictionEngineObjectsInPool;
            _expirationTime = expirationTime;

            //Create PredictionEngine Object Pool
            _predictionEnginePool = CreatePredictionEngineObjectPool();
        }

        private ObjectPool<PredictionEngine<TData, TPrediction>> CreatePredictionEngineObjectPool()
        {
            return new ObjectPool<PredictionEngine<TData, TPrediction>>(objectGenerator:() =>
                                                                            {
                                                                                //Measure PredictionEngine creation
                                                                                var watch = System.Diagnostics.Stopwatch.StartNew();

                                                                                //Make PredictionEngine
                                                                                var predEngine = _mlContext.Model.CreatePredictionEngine<TData, TPrediction>(_model);

                                                                                //Stop measuring time
                                                                                watch.Stop();
                                                                                long elapsedMs = watch.ElapsedMilliseconds;
                                                                                  
                                                                                return predEngine;
                                                                            }, 
                                                                          minPoolSize: _minPredictionEngineObjectsInPool,
                                                                          maxPoolSize: _maxPredictionEngineObjectsInPool,
                                                                          expirationTime: _expirationTime);
        }

        public TPrediction Predict(TData dataSample)
        {
            //Get PredictionEngine object from the Object Pool
            PredictionEngine<TData, TPrediction> predictionEngine = _predictionEnginePool.GetObject();

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
