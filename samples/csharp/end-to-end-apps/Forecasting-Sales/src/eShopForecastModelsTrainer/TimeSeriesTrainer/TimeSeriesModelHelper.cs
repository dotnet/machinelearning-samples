using System;
using System.IO;
using System.Linq;
using eShopForecastModelsTrainer.Data;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using static eShopForecastModelsTrainer.ConsoleHelperExt;

namespace eShopForecastModelsTrainer
{
    public class TimeSeriesModelHelper
    {
        /// <summary>
        /// Train and save model for predicting next month's product unit sales
        /// </summary>
        /// <param name="mlContext">ML.NET context.</param>
        /// <param name="dataPath">Input training data file path.</param>
        public static void PerformTimeSeriesProductForecasting(MLContext mlContext, string dataPath)
        {
            Console.WriteLine("** Testing Product 1 **");

            // Forecast units sold for product with Id == 263.
            var productId = 263;
            var productModelPath = $"product{productId}_month_timeSeriesSSA.zip";

            if (File.Exists(productModelPath))
            {
                File.Delete(productModelPath);
            }

            IDataView productDataSeries = LoadData(mlContext, productId, dataPath);
            ProductData lastMonthProductData = mlContext.Data.CreateEnumerable<ProductData>(productDataSeries, false).OrderBy(p => p.month).Last(); //TODO: If more than 1 year data, then will need to consider year too

            TrainAndSaveModel(mlContext, productDataSeries, productModelPath);
            TestPrediction(mlContext, lastMonthProductData, productModelPath);

            Console.WriteLine("** Testing Product 2 **");

            // Forecast units sold for product with Id == 988.
            productId = 988;
            productModelPath = $"product{productId}_month_timeSeriesSSA.zip";

            if (File.Exists(productModelPath))
            {
                File.Delete(productModelPath);
            }

            productDataSeries = LoadData(mlContext, productId, dataPath);
            lastMonthProductData = mlContext.Data.CreateEnumerable<ProductData>(productDataSeries, false).OrderBy(p => p.month).Last(); //TODO: If more than 1 year data, then will need to consider year too

            TrainAndSaveModel(mlContext, productDataSeries, productModelPath);
            TestPrediction(mlContext, lastMonthProductData, productModelPath);
        }

        /// <summary>
        /// Loads the monthly product data series for a product with the specified id.
        /// </summary>
        /// <param name="mlContext">ML.NET context.</param>
        /// <param name="productId">Product id.</param>
        /// <param name="dataPath">Input training data file path.</param>
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
        /// <param name="outputModelPath">Trained model path.</param>
        private static void TrainAndSaveModel(MLContext mlContext, IDataView productDataSeries, string outputModelPath)
        {
            ConsoleWriteHeader("Training product forecasting Time Series model");

            int productDataSeriesLength = mlContext.Data.CreateEnumerable<ProductData>(productDataSeries, false).Count();

            // Create and add the forecast estimator to the pipeline.
            IEstimator<ITransformer> forecastEstimator = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(ProductUnitTimeSeriesPrediction.ForecastedProductUnits), 
                inputColumnName: nameof(ProductData.units),
                windowSize:3, // TODO: This should be 12 when we have enough data; but, currently exception is thrown if the windowSize > seriesLength
                seriesLength: productDataSeriesLength, // TODO: Indicates that...
                trainSize: productDataSeriesLength, // TODO: Indicates that...
                horizon: 2, // TODO: Indicates that...
                confidenceLevel: 0.95f, // TODO: Indicates that...
                confidenceLowerBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceLowerBound), // TODO: Indicates that...
                confidenceUpperBoundColumn: nameof(ProductUnitTimeSeriesPrediction.ConfidenceUpperBound)); // TODO: Indicates that we will predict the next 3 months of sales

            // Train the forecasting model for the specified product's data series.
            ITransformer forecastTransformer = forecastEstimator.Fit(productDataSeries);

            // Create the forecast engine used for creating predictions.
            var forecastEngine = forecastTransformer.CreateTimeSeriesEngine<ProductData, ProductUnitTimeSeriesPrediction>(mlContext);

            // Save the forecasting model so that it can be loaded within an end-user app.
            forecastEngine.CheckPoint(mlContext, outputModelPath);
        }

        /// <summary>
        /// Predict samples using saved model
        /// </summary>
        /// <param name="mlContext">ML.NET context.</param>
        /// <param name="lastMonthProductData">The last month of product data in the monthly data series.</param>
        /// <param name="outputModelPath">Model file path</param>
        private static void TestPrediction(MLContext mlContext, ProductData lastMonthProductData, string outputModelPath)
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

            Console.WriteLine("** Original prediction **");

            // Get the prediction; this will include the forecasted product units sold for the next 2 months since this the time period specified in the `horizon` parameter when the forecast estimator was originally created.
            var originalSalesPrediction = forecastEngine.Predict();

            // Compare the units of the first forecasted month to the actual units sold for the next month.
            Console.WriteLine($"Product: {lastMonthProductData.productId}, Month: {lastMonthProductData.month + 1}, Year: {lastMonthProductData.year} " +  
                $"- Real Value (units): {lastMonthProductData.next}, Forecasted (units): {originalSalesPrediction.ForecastedProductUnits[0]}");

            // Get the first forecasted month's confidence interval bounds.
            Console.WriteLine($"Confidence interval: [{originalSalesPrediction.ConfidenceLowerBound[0]} - {originalSalesPrediction.ConfidenceUpperBound[0]}]\n");

            // Get the units of the second forecasted month.
            Console.WriteLine($"Product: {lastMonthProductData.productId}, Month: {lastMonthProductData.month + 2}, Year: {lastMonthProductData.year}, " +
                $"Forecasted (units): {originalSalesPrediction.ForecastedProductUnits[0]}");

            // Get the second forecasted month's confidence interval bounds.
            Console.WriteLine($"Confidence interval: [{originalSalesPrediction.ConfidenceLowerBound[0]} - {originalSalesPrediction.ConfidenceUpperBound[0]}]\n");

            Console.WriteLine("** Updated prediction **");

            // Update the forecasting model with the next month's actual product data to get an updated prediction; this time, only forecase product sales for 1 month ahead.
            ProductData newProductData = SampleProductData.MonthlyData.Where(p => p.productId == lastMonthProductData.productId).Single();
            var updatedSalesPrediction = forecastEngine.Predict(newProductData, horizon: 1);

            // Get the units of the updated forecast.
            Console.WriteLine($"Product: {lastMonthProductData.productId}, Month: {lastMonthProductData.month + 2}, Year: {lastMonthProductData.year}, " +
                $"Forecasted (units): {updatedSalesPrediction.ForecastedProductUnits[0]}");

            // Get the updated forecast's confidence interval bounds.
            Console.WriteLine($"Confidence interval: [{updatedSalesPrediction.ConfidenceLowerBound[0]} - {updatedSalesPrediction.ConfidenceUpperBound[0]}]\n");
        }
    }
}
