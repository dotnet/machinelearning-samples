using System.Threading.Tasks;

namespace eShopDashboard.Forecast
{
    public interface ICountrySales
    {
        CountrySalesPrediction Predict(string modelPath, string country, int year, int month, float max, float min, float std, int count, float sales, float med, float prev);
    }
}