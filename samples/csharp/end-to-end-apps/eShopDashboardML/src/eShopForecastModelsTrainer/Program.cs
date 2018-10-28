using System;
using System.Threading.Tasks;
using static eShopForecastModelsTrainer.ConsoleHelpers;

namespace eShopForecastModelsTrainer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                ProductModelHelper.TrainAndSaveModel("data/products.stats.csv");
                ProductModelHelper.TestPrediction();

                CountryModelHelper.TrainAndSaveModel("data/countries.stats.csv");
                CountryModelHelper.TestPrediction();
            } catch(Exception ex)
            {
                ConsoleWriteException(ex.Message);
            }
            ConsolePressAnyKey();
        }
    }
}
