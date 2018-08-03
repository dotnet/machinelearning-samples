using System;
using System.Threading.Tasks;

namespace eShopForecastModelsTrainer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await ProductModelHelper.TrainAndSaveModel("data/products.stats.csv");
                await ProductModelHelper.TestPrediction();

                await CountryModelHelper.TrainAndSaveModel("data/countries.stats.csv");
                await CountryModelHelper.TestPrediction();

                Console.Write("Hit any key to exit");
                Console.ReadLine();

            } catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}
