using Microsoft.ML;
using System;
using System.Threading.Tasks;
using static eShopForecastModelsTrainer.ConsoleHelpers;

namespace eShopForecastModelsTrainer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MLContext mlContext = new MLContext(seed: 1);  //Seed set to any number so you have a deterministic environment

                ProductModelHelper.TrainAndSaveModel(mlContext, "data/products.stats.csv");
                ProductModelHelper.TestPrediction(mlContext);

                CountryModelHelper.TrainAndSaveModel(mlContext, "data/countries.stats.csv");
                CountryModelHelper.TestPrediction(mlContext);
            } catch(Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }
            ConsolePressAnyKey();
        }
    }
}
