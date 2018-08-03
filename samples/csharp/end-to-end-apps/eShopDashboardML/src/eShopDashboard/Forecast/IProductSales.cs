using System.Threading.Tasks;

namespace eShopDashboard.Forecast
{
    public interface IProductSales
    {
        Task<ProductUnitPrediction> Predict(string modelPath, string productId, int year, int month, float units, float avg, int count, float max, float min, float prev);
    }
}