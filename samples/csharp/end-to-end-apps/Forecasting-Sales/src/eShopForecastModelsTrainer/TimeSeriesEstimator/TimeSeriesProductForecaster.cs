using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using eShopForecastModelsTrainer.Data;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using static eShopForecastModelsTrainer.ConsoleHelperExt;


namespace eShopForecastModelsTrainer
{
    public class TimeSeriesProductForecaster
    {
        /// <summary>
        /// Train and save model for predicting next month's product unit sales
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <param name="outputForecasterPath">Trained model path</param>
        public static void TrainAndSaveModel(MLContext mlContext, string dataPath, string outputForecasterPath = "product_month_timeSeriesSSA.zip")
        {
            if (File.Exists(outputForecasterPath))
            {
                File.Delete(outputForecasterPath);
            }

            var productId1 = 263;
            var productIdModelPath1 = $"product{productId1}_month_timeSeriesSSA.zip";
            CreateProductModelUsingPipeline(mlContext, productId1, dataPath, productIdModelPath1);
            TestPrediction(mlContext, SampleProductData.Product1MonthlyData[1], productIdModelPath1);

            var productId2 = 988;
            var productIdModelPath2 = $"product{productId2}_month_timeSeriesSSA.zip";
            CreateProductModelUsingPipeline(mlContext, productId2, dataPath, productIdModelPath2);
            TestPrediction(mlContext, SampleProductData.Product2MonthlyData[1], productIdModelPath2);
        }

        /// <summary>
        /// Build model for predicting next month's product unit sales using time series forecasting.
        /// </summary>
        /// <param name="productId">Id of the product that will be used in forecasting.</param>
        /// <param name="dataPath">Input training file path.</param>
        /// <param name="outputModelPath">Trained model path.</param>
        private static void CreateProductModelUsingPipeline(MLContext mlContext, float productId, string dataPath, string outputModelPath)
        {
            ConsoleWriteHeader("Training product forecasting Time Series model");

            // Load the data series for the specific product that will be used for forecasting sales.
            var productDataView = mlContext.Data.LoadFromTextFile<ProductData>(dataPath, hasHeader: true, separatorChar: ',');
            var singleProductDataView = mlContext.Data.FilterRowsByColumn(productDataView, nameof(ProductData.productId), productId, productId + 1);
            var productSeriesLength = mlContext.Data.CreateEnumerable<ProductData>(singleProductDataView, false).Count();
       
            // Create and add the forecast estimator to the pipeline.
            IEstimator<ITransformer> forecastEstimator = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(ProductUnitTimeSeriesPrediction.ForecastedProductUnits), 
                inputColumnName: nameof(ProductData.units),
                windowSize:3, // TODO: This should be 12 when we have enough data; but, currently exception is thrown if the windowSize > seriesLength
                seriesLength: productSeriesLength, // TODO: Indicates that...
                trainSize: productSeriesLength, // TODO: Indicates that...
                horizon: 2, // TODO: Indicates that...
                confidenceLevel: 0.95f, // TODO: Indicates that...
                confidenceLowerBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceLowerBound), // TODO: Indicates that...
                confidenceUpperBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceUpperBound)); // TODO: Indicates that we will predict the next 3 months of sales

            // Train the forecasting model for the specified product's data series.
            ITransformer forecastTransformer = forecastEstimator.Fit(singleProductDataView);

            // Create the forecast engine used for creating predictions.
            var forecastEngine = forecastTransformer.CreateTimeSeriesEngine<ProductData, ProductUnitTimeSeriesPrediction>(mlContext);

            // Save the forecasting model so that it can be loaded within an end-user app.
            forecastEngine.CheckPoint(mlContext, outputModelPath);
        }

        /// <summary>
        /// Predict samples using saved model
        /// </summary>
        /// <param name="outputModelPath">Model file path</param>
        public static void TestPrediction(MLContext mlContext, ProductData forecastProductData, string outputModelPath)
        {
            ConsoleWriteHeader("Testing Product Unit Sales Forecast Time Series model");

            // Load the forecast engine that has been previously saved

            ITransformer forecaster;
            using (var file = File.OpenRead(outputModelPath))
            {
                forecaster = mlContext.Model.Load(file, out DataViewSchema schema);
            }

            // We must create a new prediction engine from the persisted model.
            var forecastEngine = forecaster.CreateTimeSeriesEngine<ProductData, ProductUnitTimeSeriesPrediction>(mlContext);

            // Get the forecasted prediction; this will predict the product's sales for the next 2 months since this the time period specified in the `horizon` parameter when the forecast estimator was originally created.
            var forecast1 = forecastEngine.Predict();

            // Compare the results of the first prediction against the actual product sales for the next month.
            Console.WriteLine("Number of predictions: " + forecast1.ForecastedProductUnits.Count());
            Console.WriteLine($"Product: {forecastProductData.productId}, Month: {forecastProductData.month + 1}, Year: {forecastProductData.year} - Real Value (units): {forecastProductData.units}, Forecasting (units): {forecast1.ForecastedProductUnits[0]}");

            PrintForecastValuesAndIntervals(forecast1.ConfidenceLowerBound, forecast1.ConfidenceUpperBound);

            // Update the forecasting model with the next month's actual data and get the next prediction; compare this with the previous prediction???  
            var forecast2 = forecastEngine.Predict(forecastProductData, horizon: 1);

            Console.WriteLine("Number of predictions: " + forecast2.ForecastedProductUnits.Count());
            Console.WriteLine($"First forecast, 2nd month prediction: {forecast1.ForecastedProductUnits[1]} - Second forecast, 2nd month prediction: {forecast2.ForecastedProductUnits[0]}");
            Console.WriteLine($"Product: {forecastProductData.productId}, Month: {forecastProductData.month + 2}, year: {forecastProductData.year} - Forecasting (units): {forecast2.ForecastedProductUnits[0]}");

            PrintForecastValuesAndIntervals(forecast2.ConfidenceLowerBound, forecast2.ConfidenceUpperBound);
        }

        static void PrintForecastValuesAndIntervals(float[] confidenceIntervalLowerBounds, float[] confidenceIntervalUpperBounds)
        {
            Console.WriteLine($"Confidence intervals:");
            for (int index = 0; index < confidenceIntervalLowerBounds.Length; index++)
                Console.Write($"[{confidenceIntervalLowerBounds[index]} -" +
                    $" {confidenceIntervalUpperBounds[index]}] ");
            Console.WriteLine();
        }
    }
}
