using Common;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using static eShopForecastModelsTrainer.ConsoleHelperExt;

namespace eShopForecastModelsTrainer
{
    class Program
    {
        private static readonly string BaseDatasetsRelativePath = @"../../../Data";
        private static readonly string ProductDataRelativePath = $"{BaseDatasetsRelativePath}/products.stats.csv";
        private static readonly string ProductDataPath = GetAbsolutePath(ProductDataRelativePath);
        private static readonly string BaseDatasets2RelativePath =
                @"../../../../eShopDashboard/Infrastructure/Setup/DataFiles";

        static void Main(string[] args)
        {

            var datasetFile = "ForecastingSalesEShopDashboardOrderDataset";
            var datasetZip = datasetFile + ".zip";
            var datasetUrl = "https://bit.ly/389qjAQ";
            var commonDatasetsRelativePath = @"../../../../../../../../../datasets";
            var commonDatasetsPath = GetAbsolutePath(commonDatasetsRelativePath);
            var path1 = Path.GetFullPath(Path.Combine(BaseDatasets2RelativePath, "Orders.csv"));
            var path2 = Path.GetFullPath(Path.Combine(BaseDatasets2RelativePath, "OrderItems.csv"));
            var path3 = Path.GetFullPath(Path.Combine(BaseDatasets2RelativePath, "CatalogItems.csv"));
            var path4 = Path.GetFullPath(Path.Combine(BaseDatasets2RelativePath, "CatalogTags.txt"));
            List<string> destFiles = new List<string>() { path1 };
            Web.DownloadBigFile(BaseDatasets2RelativePath, datasetUrl, datasetZip, commonDatasetsPath, destFiles);

            var dataset2File = "ForecastingSalesOnlineRetailDataset";
            var dataset2Zip = dataset2File + ".zip";
            var dataset2Url = "https://bit.ly/3yjp8cK";
            List<string> destFiles2 = new List<string>() { ProductDataPath };
            Web.DownloadBigFile(BaseDatasetsRelativePath, dataset2Url, dataset2Zip, commonDatasetsPath, destFiles2);

            try
            {
                // This sample shows two different ML tasks and algorithms that can be used for forecasting:
                // 1.) Regression using FastTreeTweedie Regression
                // 2.) Time Series using Single Spectrum Analysis
                // Each of these techniques are used to forecast monthly units for the same products so that you can compare the forecasts.

                var mlContext = new MLContext(seed: 1);  //Seed set to any number so you have a deterministic environment

                ConsoleWriteHeader("Forecast using Regression model");

                RegressionProductModelHelper.TrainAndSaveModel(mlContext, ProductDataPath);
                RegressionProductModelHelper.TestPrediction(mlContext);

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
            var _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativeDatasetPath);

            return fullPath;
        }
    }
}
