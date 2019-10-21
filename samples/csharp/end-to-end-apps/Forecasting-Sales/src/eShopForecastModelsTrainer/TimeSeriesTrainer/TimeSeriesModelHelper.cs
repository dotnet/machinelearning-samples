using System;
using System.IO;
using System.Linq;
using eShopForecast;
using eShopForecastModelsTrainer.Data;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using static eShopForecastModelsTrainer.ConsoleHelperExt;

namespace eShopForecastModelsTrainer
{
    public class TimeSeriesModelHelper
    {
        /// <summary>
        /// Predicts future product sales using time series forecasting with SSA (single spectrum analysis).
        /// </summary>
        /// <param name="mlContext">ML.NET context.</param>
        /// <param name="dataPath">Input data file path.</param>
        public static void PerformTimeSeriesProductForecasting(MLContext mlContext, string dataPath)
        {
            Console.WriteLine("=============== Forecasting Product Units ===============");

            // Forecast units sold for product with Id == 988.
            var productId = 988;
            ForecastProductUnits(mlContext, productId, dataPath);
        }

        /// <summary>
        /// Fit and save checkpoint of the model for predicting future product sales.
        /// </summary>
        /// <param name="mlContext">ML.NET context.</param>
        /// <param name="productId">Id of the product series to forecast.</param>
        /// <param name="dataPath">Input data file path.</param>
        private static void ForecastProductUnits(MLContext mlContext, int productId, string dataPath)
        {
            var productModelPath = $"product{productId}_month_timeSeriesSSA.zip";

            if (File.Exists(productModelPath))
            {
                File.Delete(productModelPath);
            }

            IDataView productDataView = LoadData(mlContext, productId, dataPath);
            var singleProductDataSeries = mlContext.Data.CreateEnumerable<ProductData>(productDataView, false).OrderBy(p => p.year).ThenBy(p => p.month);
            ProductData lastMonthProductData = singleProductDataSeries.Last();

            FitAndSaveModel(mlContext, productDataView, productModelPath);
            TestPrediction(mlContext, lastMonthProductData, productModelPath);
        }

        /// <summary>
        /// Loads the monthly product data series for a product with the specified id.
        /// </summary>
        /// <param name="mlContext">ML.NET context.</param>
        /// <param name="productId">Product id.</param>
        /// <param name="dataPath">Input data file path.</param>
        private static IDataView LoadData(MLContext mlContext, float productId, string dataPath)
        {
            // Load the data series for the specific product that will be used for forecasting sales.
            IDataView allProductsDataView = mlContext.Data.LoadFromTextFile<ProductData>(dataPath, hasHeader: true, separatorChar: ',');
            IDataView productDataView = mlContext.Data.FilterRowsByColumn(allProductsDataView, nameof(ProductData.productId), productId, productId + 1);

            return productDataView;
        }

        /// <summary>
        /// Build model for predicting next month's product unit sales using time series forecasting.
        /// </summary>
        /// <param name="mlContext">ML.NET context.</param>
        /// <param name="productDataSeries">ML.NET IDataView representing the loaded product data series.</param>
        /// <param name="outputModelPath">Model path.</param>
        private static void FitAndSaveModel(MLContext mlContext, IDataView productDataSeries, string outputModelPath)
        {
            ConsoleWriteHeader("Fitting product forecasting Time Series model");

            const int numSeriesDataPoints = 34; //The underlying data has a total of 34 months worth of data for each product

            // Create and add the forecast estimator to the pipeline.
            IEstimator<ITransformer> forecastEstimator = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(ProductUnitTimeSeriesPrediction.ForecastedProductUnits), 
                inputColumnName: nameof(ProductData.units), // This is the column being forecasted.
                windowSize: 12, // Window size is set to the time period represented in the product data cycle; our product cycle is based on 12 months, so this is set to a factor of 12, e.g. 3.
                seriesLength: numSeriesDataPoints, // This parameter specifies the number of data points that are used when performing a forecast.
                trainSize: numSeriesDataPoints, // This parameter specifies the total number of data points in the input time series, starting from the beginning.
                horizon: 2, // Indicates the number of values to forecast; 2 indicates that the next 2 months of product units will be forecasted.
                confidenceLevel: 0.95f, // Indicates the likelihood the real observed value will fall within the specified interval bounds.
                confidenceLowerBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceLowerBound), //This is the name of the column that will be used to store the lower interval bound for each forecasted value.
                confidenceUpperBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceUpperBound)); //This is the name of the column that will be used to store the upper interval bound for each forecasted value.

            // Fit the forecasting model to the specified product's data series.
            ITransformer forecastTransformer = forecastEstimator.Fit(productDataSeries);

            // Create the forecast engine used for creating predictions.
            TimeSeriesPredictionEngine<ProductData, ProductUnitTimeSeriesPrediction> forecastEngine = forecastTransformer.CreateTimeSeriesEngine<ProductData, ProductUnitTimeSeriesPrediction>(mlContext);

            // Save the forecasting model so that it can be loaded within an end-user app.
            forecastEngine.CheckPoint(mlContext, outputModelPath);
        }

        /// <summary>
        /// Predict samples using saved model.
        /// </summary>
        /// <param name="mlContext">ML.NET context.</param>
        /// <param name="lastMonthProductData">The last month of product data in the monthly data series.</param>
        /// <param name="outputModelPath">Model file path</param>
        private static void TestPrediction(MLContext mlContext, ProductData lastMonthProductData, string outputModelPath)
        {
            ConsoleWriteHeader("Testing product unit sales forecast Time Series model");

            // Load the forecast engine that has been previously saved.
            ITransformer forecaster;
            using (var file = File.OpenRead(outputModelPath))
            {
                forecaster = mlContext.Model.Load(file, out DataViewSchema schema);
            }

            // We must create a new prediction engine from the persisted model.
            TimeSeriesPredictionEngine<ProductData, ProductUnitTimeSeriesPrediction> forecastEngine = forecaster.CreateTimeSeriesEngine<ProductData, ProductUnitTimeSeriesPrediction>(mlContext);

            // Get the prediction; this will include the forecasted product units sold for the next 2 months since this the time period specified in the `horizon` parameter when the forecast estimator was originally created.
            Console.WriteLine("\n** Original prediction **");
            ProductUnitTimeSeriesPrediction originalSalesPrediction = forecastEngine.Predict();

            // Compare the units of the first forecasted month to the actual units sold for the next month.
            var predictionMonth = lastMonthProductData.month == 12 ? 1 : lastMonthProductData.month + 1;
            var predictionYear = predictionMonth < lastMonthProductData.month ? lastMonthProductData.year + 1 : lastMonthProductData.year;
            Console.WriteLine($"Product: {lastMonthProductData.productId}, Month: {predictionMonth}, Year: {predictionYear} " +  
                $"- Real Value (units): {lastMonthProductData.next}, Forecast Prediction (units): {originalSalesPrediction.ForecastedProductUnits[0]}");

            // Get the first forecasted month's confidence interval bounds.
            Console.WriteLine($"Confidence interval: [{originalSalesPrediction.ConfidenceLowerBound[0]} - {originalSalesPrediction.ConfidenceUpperBound[0]}]\n");

            // Get the units of the second forecasted month.
            Console.WriteLine($"Product: {lastMonthProductData.productId}, Month: {lastMonthProductData.month + 2}, Year: {lastMonthProductData.year}, " +
                $"Forecast (units): {originalSalesPrediction.ForecastedProductUnits[1]}");

            // Get the second forecasted month's confidence interval bounds.
            Console.WriteLine($"Confidence interval: [{originalSalesPrediction.ConfidenceLowerBound[1]} - {originalSalesPrediction.ConfidenceUpperBound[1]}]\n");

            // Update the forecasting model with the next month's actual product data to get an updated prediction; this time, only forecast product sales for 1 month ahead.
            Console.WriteLine("** Updated prediction **");
            ProductUnitTimeSeriesPrediction updatedSalesPrediction = forecastEngine.Predict(SampleProductData.MonthlyData[1], horizon: 1);

            // Save a checkpoint of the forecasting model.
            forecastEngine.CheckPoint(mlContext, outputModelPath);

            // Get the units of the updated forecast.
            predictionMonth = lastMonthProductData.month >= 11 ? (lastMonthProductData.month + 2) % 12 : lastMonthProductData.month + 2;
            predictionYear = predictionMonth < lastMonthProductData.month ? lastMonthProductData.year + 1 : lastMonthProductData.year;
            Console.WriteLine($"Product: {lastMonthProductData.productId}, Month: {predictionMonth}, Year: {predictionYear}, " +
                $"Forecast (units): {updatedSalesPrediction.ForecastedProductUnits[0]}");

            // Get the updated forecast's confidence interval bounds.
            Console.WriteLine($"Confidence interval: [{updatedSalesPrediction.ConfidenceLowerBound[0]} - {updatedSalesPrediction.ConfidenceUpperBound[0]}]\n");
        }
    }
}
