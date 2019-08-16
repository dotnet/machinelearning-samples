using eShopForecastModelsTrainer.Data;
using Microsoft.ML;
using System;
using System.IO;
using static eShopForecastModelsTrainer.ConsoleHelperExt;

namespace eShopForecastModelsTrainer
{
    class Program
    {
        private static readonly string BaseDatasetsRelativePath = @"../../../Data";
        private static readonly string CountryDataRealtivePath = $"{BaseDatasetsRelativePath}/countries.stats.csv";
        private static readonly string ProductDataRealtivePath = $"{BaseDatasetsRelativePath}/products.stats.csv";

        private static readonly string CountryDataPath = GetAbsolutePath(CountryDataRealtivePath);
        private static readonly string ProductDataPath = GetAbsolutePath(ProductDataRealtivePath);

        static void Main(string[] args)
        {
            try
            {
                // This sample shows two different ML tasks and algorithms that can be used for forecasting:
                // 1.) Regression using FastTreeTweedie Regression
                // 2.) Time Series using Single Spectrum Analysis
                // Each of these techniques are used to forecast monthly units for the same products so that you can compare the forecasts.

                MLContext mlContext = new MLContext(seed: 1);  //Seed set to any number so you have a deterministic environment

                ConsoleWriteHeader("Forecast using Regression model");

                RegressionProductModelHelper.TrainAndSaveModel(mlContext, ProductDataPath);
                RegressionProductModelHelper.TestPrediction(mlContext);

                RegressionCountryModelHelper.TrainAndSaveModel(mlContext, CountryDataPath);
                RegressionCountryModelHelper.TestPrediction(mlContext);

                ConsoleWriteHeader("Forecast using Time Series SSA estimation");

                TimeSeriesModelHelper.PerformTimeSeriesProductForecasting(mlContext, ProductDataPath);
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex.ToString());
            }
            ConsolePressAnyKey();
        }

        public static string GetAbsolutePath(string relativeDatasetPath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativeDatasetPath);

            return fullPath;
        }
    }
}
