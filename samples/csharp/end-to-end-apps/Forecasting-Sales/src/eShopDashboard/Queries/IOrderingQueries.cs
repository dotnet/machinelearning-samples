using eShopForecast;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShopDashboard.Queries
{
    public interface IOrderingQueries
    {
        Task<IEnumerable<dynamic>> GetProductHistoryAsync(string productId);

        Task<IEnumerable<dynamic>> GetProductStatsAsync(string productId);

        Task<IEnumerable<dynamic>> GetProductStatsAsync();

        Task<dynamic[]> GetProductsHistoryDepthAsync(IEnumerable<int> products);

        Task<IEnumerable<ProductData>> GetProductDataAsync(string productId);
    }
}