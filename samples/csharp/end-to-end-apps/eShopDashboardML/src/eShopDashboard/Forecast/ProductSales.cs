
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;
using System.IO;

namespace eShopDashboard.Forecast
{
    /// <summary>
    /// This is the input to the trained model.
    /// </summary>
    public class ProductData
    {
        // next,productId,year,month,units,avg,count,max,min,prev
        public ProductData(string productId, int year, int month, float units, float avg, 
            int count, float max, float min, float prev)
        {
            this.productId = productId;
            this.year = year;
            this.month = month;
            this.units = units;
            this.avg = avg;
            this.count = count;
            this.max = max;
            this.min = min;
            this.prev = prev;
        }

        public float next;

        public string productId;

        public float year;
        public float month;
        public float units;
        public float avg;
        public float count;
        public float max;
        public float min;
        public float prev;
    }

    /// <summary>
    /// This is the output of the scored model, the prediction.
    /// </summary>
    public class ProductUnitPrediction
    {
        // Below columns are produced by the model's predictor.
        public float Score;
    }

    public class ProductSales : IProductSales
    {
        /// <summary>
        /// This method demonstrates how to run prediction on one example at a time.
        /// </summary>
        public ProductUnitPrediction Predict(string modelPath, string productId, int year, int month, float units, float avg, int count, float max, float min, float prev)
        {
            // Load model
            var predictionEngine = CreatePredictionEngineAsync(modelPath);

            // Build country sample
            var inputExample = new ProductData(productId, year, month, units, avg, count, max, min, prev);

            // Returns prediction
            var productUnitsPrediction = predictionEngine.Predict(inputExample);
            return productUnitsPrediction;
        }

        /// <summary>
        /// This function creates a prediction engine from the model located in the <paramref name="modelPath"/>.
        /// </summary>
        private PredictionFunction<ProductData, ProductUnitPrediction> CreatePredictionEngineAsync(string modelPath)
        {
            var env = new LocalEnvironment(seed: 1);  //Seed set to any number so you have a deterministic environment
            ITransformer model;
            using (var file = File.OpenRead(modelPath))
            {
                model = TransformerChain
                    .LoadFrom(env, file);
            }

            return model.MakePredictionFunction<ProductData, ProductUnitPrediction>(env);
        }
    }
}
