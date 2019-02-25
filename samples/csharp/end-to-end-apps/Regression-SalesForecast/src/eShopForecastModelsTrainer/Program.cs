using Microsoft.ML;
using System;
using System.IO;
using System.Threading.Tasks;
using static eShopForecastModelsTrainer.ConsoleHelpers;

namespace eShopForecastModelsTrainer
{
    class Program
    {
        private static readonly string BaseDatasetsRelativePath = @"../../../../Data";
        private static readonly string CountryDataRealtivePath = $"{BaseDatasetsRelativePath}/countries.stats.csv";
        private static readonly string ProductDataRealtivePath = $"{BaseDatasetsRelativePath}/products.stats.csv";

        private static readonly string CountryDataPath = GetDataSetAbsolutePath(CountryDataRealtivePath);
        private static readonly string ProductDataPath = GetDataSetAbsolutePath(ProductDataRealtivePath);

        static void Main(string[] args)
        {
            
            try
            {
                MLContext mlContext = new MLContext(seed: 1);  //Seed set to any number so you have a deterministic environment
                
                ProductModelHelper.TrainAndSaveModel(mlContext, ProductDataPath);
                ProductModelHelper.TestPrediction(mlContext);

                CountryModelHelper.TrainAndSaveModel(mlContext, CountryDataPath);
                CountryModelHelper.TestPrediction(mlContext);
            } catch(Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }
            ConsolePressAnyKey();
        }

        public static string GetDataSetAbsolutePath(string relativeDatasetPath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath + "/" + relativeDatasetPath);

            return fullPath;
        }
    }
}
