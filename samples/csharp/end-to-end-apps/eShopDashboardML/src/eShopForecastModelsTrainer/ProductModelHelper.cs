using ML = Microsoft.ML;
using Microsoft.ML;
//using Microsoft.ML.Runtime;
//using Microsoft.ML.Runtime.Data;
//using Microsoft.ML.Runtime.EntryPoints;
using Microsoft.ML.Data;
using System;
using System.IO;
using System.Threading.Tasks;

namespace eShopForecastModelsTrainer
{
    public class ProductModelHelper
    {
        /// <summary>
        /// Train and save model for predicting next month country unit sales
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <param name="outputModelPath">Trained model path</param>
        public static async Task TrainAndSaveModel(string dataPath, string outputModelPath = "product_month_fastTreeTweedie.zip")
        {
            if (File.Exists(outputModelPath))
            {
                File.Delete(outputModelPath);
            }

            var model = CreateProductModelUsingPipeline(dataPath);

            await model.WriteAsync(outputModelPath);
        }


        /// <summary>
        /// Build model for predicting next month country unit sales using Learning Pipelines API
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <returns></returns>
        private static PredictionModel<ProductData, ProductUnitPrediction> 
                        CreateProductModelUsingPipeline(string dataPath)
        {
            Console.WriteLine("*************************************************");
            Console.WriteLine("Training product forecasting model using Pipeline");

            var learningPipeline = new LearningPipeline();

            // First stage in the pipeline will be reading the source csv file
            learningPipeline.Add(new TextLoader(dataPath).CreateFrom<ProductData>(useHeader: true, separator: ','));

            // The model needs the columns to be arranged into a single column of numeric type
            // First, we group all numeric columns into a single array named NumericalFeatures
            learningPipeline.Add(new ML.Transforms.ColumnConcatenator(
                outputColumn: "NumericalFeatures",
                nameof(ProductData.year),
                nameof(ProductData.month),
                nameof(ProductData.max),
                nameof(ProductData.min),
                nameof(ProductData.count),
                nameof(ProductData.units),
                nameof(ProductData.avg),
                nameof(ProductData.prev)
            ));

            // Second group is for categorical features (just one in this case), we name this column CategoryFeatures
            learningPipeline.Add(new ML.Transforms.ColumnConcatenator(outputColumn: "CategoryFeatures", nameof(ProductData.productId)));

            // Then we need to transform the category column using one-hot encoding. This will return a numeric array
            learningPipeline.Add(new ML.Transforms.CategoricalOneHotVectorizer("CategoryFeatures"));

            // Once all columns are numeric types, all columns will be combined
            // into a single column, named Features 
            learningPipeline.Add(new ML.Transforms.ColumnConcatenator(outputColumn: "Features", "NumericalFeatures", "CategoryFeatures"));

            // Add the Learner to the pipeline. The Learner is the machine learning algorithm used to train a model
            // In this case, FastTreeTweedieRegressor was one of the best performing algorithms, but you can 
            // choose any other regression algorithm (StochasticDualCoordinateAscentRegressor,PoissonRegressor,...)
            learningPipeline.Add(new ML.Trainers.FastTreeTweedieRegressor { NumThreads = 1, FeatureColumn = "Features" });

            // Finally, we train the pipeline using the training dataset set at the first stage
            var model = learningPipeline.Train<ProductData, ProductUnitPrediction>();

            return model;
        }

        /// <summary>
        /// Predict samples using saved model
        /// </summary>
        /// <param name="outputModelPath">Model file path</param>
        /// <returns></returns>
        public static async Task TestPrediction(string outputModelPath = "product_month_fastTreeTweedie.zip")
        {
            Console.WriteLine("*********************************");
            Console.WriteLine("Testing Product Unit Sales Forecast model");

            // Read the model that has been previously saved by the method SaveModel
            var model = await PredictionModel.ReadAsync<ProductData, ProductUnitPrediction>(outputModelPath);

            Console.WriteLine("** Testing Product 1 **");

            // Build sample data
            ProductData dataSample = new ProductData()
            {
                productId = "263", month = 10, year = 2017, avg = 91, max = 370, min = 1,
                count = 10, prev = 1675, units = 910
            };

            //model.Predict() predicts the nextperiod/month forecast to the one provided
            ProductUnitPrediction prediction = model.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month+1}, year: {dataSample.year} - Real value (units): 551, Forecast Prediction (units): {prediction.Score}");

            dataSample = new ProductData()
            {
                productId = "263", month = 11, year = 2017, avg = 29, max = 221, min = 1,
                count = 35, prev = 910, units = 551
            };

            //model.Predict() predicts the nextperiod/month forecast to the one provided
            prediction = model.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month+1}, year: {dataSample.year} - Forecast Prediction (units): {prediction.Score}");

            Console.WriteLine(" ");

            Console.WriteLine("** Testing Product 2 **");

            dataSample = new ProductData()
            {
                productId = "988", month = 10, year = 2017, avg = 43, max = 220, min = 1,
                count = 25, prev = 1036, units = 1094
            };

            prediction = model.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month+1}, year: {dataSample.year} - Real Value (units): 1076, Forecasting (units): {prediction.Score}");

            dataSample = new ProductData()
            {
                productId = "988", month = 11, year = 2017, avg = 41, max = 225, min = 4,
                count = 26, prev = 1094, units = 1076
            };

            prediction = model.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month+1}, year: {dataSample.year} - Forecasting (units): {prediction.Score}");
        }
    }
}
