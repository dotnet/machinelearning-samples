using Microsoft.ML;
using Microsoft.Extensions.ObjectPool;
using System.IO;
using Microsoft.Data.DataView;

namespace Scalable.Model.Engine
{   
    public class MLModelEngine<TData, TPrediction> 
                    where TData : class
                    where TPrediction : class, new()
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _mlModel;
        private readonly ObjectPool<PredictionEngine<TData, TPrediction>> _predictionEnginePool;
        private readonly int _maxObjectsRetained;

        /// <summary>
        /// Exposing the ML model allowing additional ITransformer operations such as Bulk predictions', etc.
        /// </summary>
        public ITransformer MLModel
        {
            get => _mlModel;
        }

        /// <summary>
        /// Constructor with modelFilePathName to load from
        /// </summary>
        public MLModelEngine(string modelFilePathName, int maxObjectsRetained = -1)
        {
            //Create the MLContext object to use under the scope of this class 
            _mlContext = new MLContext();

            //Load the ProductSalesForecast model from the .ZIP file
            using (var fileStream = File.OpenRead(modelFilePathName))
            {
                _mlModel = _mlContext.Model.Load(fileStream);
            }

            _maxObjectsRetained = maxObjectsRetained;

            //Create PredictionEngine Object Pool
            _predictionEnginePool = CreatePredictionEngineObjectPool();
        }

        // Create the Object Pool based on the PooledPredictionEnginePolicy.
        // This method is only used once, from the cosntructor.
        private ObjectPool<PredictionEngine<TData, TPrediction>> CreatePredictionEngineObjectPool()
        {
            var predEnginePolicy = new PooledPredictionEnginePolicy<TData, TPrediction>(_mlContext, _mlModel);

            DefaultObjectPool<PredictionEngine<TData, TPrediction>> pool;

            if (_maxObjectsRetained != -1)
            {
                pool = new DefaultObjectPool<PredictionEngine<TData, TPrediction>>(predEnginePolicy, _maxObjectsRetained);
            }
            else
            {
                //default maximumRetained is Environment.ProcessorCount * 2, if not explicitly provided
                pool = new DefaultObjectPool<PredictionEngine<TData, TPrediction>>(predEnginePolicy);
            }

            return pool;
        }

        /// <summary>
        /// The Predict() method performs a single prediction based on sample data provided (dataSample) and returning the Prediction.
        /// This implementation uses an object pool internally so it is optimized for scalable and multi-threaded apps.
        /// </summary>
        /// <param name="dataSample"></param>
        /// <returns></returns>
        public TPrediction Predict(TData dataSample)
        {
            //Get PredictionEngine object from the Object Pool
            PredictionEngine<TData, TPrediction> predictionEngine = _predictionEnginePool.Get();

            try
            {
                //Predict
                TPrediction prediction = predictionEngine.Predict(dataSample);
                return prediction;
            }
            finally
            {
                //Release used PredictionEngine object into the Object Pool
                _predictionEnginePool.Return(predictionEngine);
            }
        }

    }
    
}
