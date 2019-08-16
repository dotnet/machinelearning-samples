using eShopForecast;
using Microsoft.ML;
using System;
using System.IO;
using System.Linq;
using static eShopForecastModelsTrainer.ConsoleHelperExt;
using Common;
using Microsoft.ML.Data;

namespace eShopForecastModelsTrainer
{
    public class RegressionProductModelHelper
    {
        /// <summary>
        /// Train and save model for predicting the next month's product unit sales
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <param name="outputModelPath">Trained model path</param>
        public static void TrainAndSaveModel(MLContext mlContext, string dataPath, string outputModelPath = "product_month_fastTreeTweedie.zip")
        {
            if (File.Exists(outputModelPath))
            {
                File.Delete(outputModelPath);
            }

            CreateProductModelUsingPipeline(mlContext, dataPath, outputModelPath);
        }

        /// <summary>
        /// Build model for predicting next month's product unit sales using Learning Pipelines API
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        private static void CreateProductModelUsingPipeline(MLContext mlContext, string dataPath, string outputModelPath)
        {
            ConsoleWriteHeader("Training product forecasting Regression model");

            var trainingDataView = mlContext.Data.LoadFromTextFile<ProductData>(dataPath, hasHeader: true, separatorChar:',');

            var trainer = mlContext.Regression.Trainers.FastTreeTweedie(labelColumnName: "Label", featureColumnName: "Features");

            var trainingPipeline = mlContext.Transforms.Concatenate(outputColumnName: "NumFeatures", nameof(ProductData.year), nameof(ProductData.month), nameof(ProductData.units), nameof(ProductData.avg), nameof(ProductData.count), 
                nameof(ProductData.max), nameof(ProductData.min), nameof(ProductData.prev) )
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CatFeatures", inputColumnName: nameof(ProductData.productId)))
                .Append(mlContext.Transforms.Concatenate(outputColumnName: "Features", "NumFeatures", "CatFeatures"))
                .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(ProductData.next)))
                .Append(trainer);
            
            // Cross-Validate with single dataset (since we don't have two datasets, one for training and for evaluate)
            // in order to evaluate and get the model's accuracy metrics
            Console.WriteLine("=============== Cross-validating to get Regression model's accuracy metrics ===============");
            var crossValidationResults = mlContext.Regression.CrossValidate(data:trainingDataView, estimator:trainingPipeline, numberOfFolds: 6, labelColumnName: "Label");
            ConsoleHelper.PrintRegressionFoldsAverageMetrics(trainer.ToString(), crossValidationResults);

            // Train the model
            var model = trainingPipeline.Fit(trainingDataView);

            // Save the model for later comsumption from end-user apps
            mlContext.Model.Save(model, trainingDataView.Schema, outputModelPath);
        }

        /// <summary>
        /// Predict samples using saved model
        /// </summary>
        /// <param name="outputModelPath">Model file path</param>
        public static void TestPrediction(MLContext mlContext, string outputModelPath = "product_month_fastTreeTweedie.zip")
        {
            ConsoleWriteHeader("Testing Product Unit Sales Forecast Regression model");

            // Read the model that has been previously saved by the method SaveModel

            ITransformer trainedModel;
            using (var stream = File.OpenRead(outputModelPath))
            {
                trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);
            }

            var predictionEngine = mlContext.Model.CreatePredictionEngine<ProductData, ProductUnitRegressionPrediction>(trainedModel);

            Console.WriteLine("** Testing Product 1 **");

            // Build sample data
            ProductData dataSample = new ProductData()
            {
                productId = 263,
                month = 10,
                year = 2017,
                avg = 91,
                max = 370,
                min = 1,
                count = 10,
                prev = 1675,
                units = 910
            };

            // Predict the nextperiod/month forecast to the one provided
            ProductUnitRegressionPrediction prediction = predictionEngine.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Real value (units): 551, Forecast Prediction (units): {prediction.Score}");

            dataSample = new ProductData()
            {
                productId = 263,
                month = 11,
                year = 2017,
                avg = 29,
                max = 221,
                min = 1,
                count = 35,
                prev = 910,
                units = 551
            };

            // Predicts the nextperiod/month forecast to the one provided
            prediction = predictionEngine.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Forecast Prediction (units): {prediction.Score}");

            Console.WriteLine(" ");

            Console.WriteLine("** Testing Product 2 **");

            dataSample = new ProductData()
            {
                productId = 988,
                month = 10,
                year = 2017,
                avg = 43,
                max = 220,
                min = 1,
                count = 25,
                prev = 1036,
                units = 1094
            };

            prediction = predictionEngine.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Real Value (units): 1076, Forecasting (units): {prediction.Score}");

            dataSample = new ProductData()
            {
                productId = 988,
                month = 11,
                year = 2017,
                avg = 41,
                max = 225,
                min = 4,
                count = 26,
                prev = 1094,
                units = 1076
            };

            prediction = predictionEngine.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month + 1}, year: {dataSample.year} - Forecasting (units): {prediction.Score}");
        }
    }
}
