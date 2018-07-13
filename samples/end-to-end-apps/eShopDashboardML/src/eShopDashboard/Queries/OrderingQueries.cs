using Dapper;
using eShopDashboard.Infrastructure.Data.Ordering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace eShopDashboard.Queries
{
    public class OrderingQueries : IOrderingQueries
    {
        private readonly string _connectionString;
        private readonly OrderingContext _orderingContext;

        public OrderingQueries(OrderingContext orderingContext)
        {
            _orderingContext = orderingContext;
            _connectionString = _orderingContext.Database.GetDbConnection().ConnectionString;
        }

        public async Task<IEnumerable<dynamic>> GetCountryHistoryAsync(string country)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return await connection.QueryAsync<dynamic>(OrderingQueriesText.CountryHistory(country), new { country });
            }
        }

        public async Task<IEnumerable<dynamic>> GetProductHistoryAsync(string productId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return await connection.QueryAsync<dynamic>(OrderingQueriesText.ProductHistory(productId), new { productId });
            }
        }

        public async Task<IEnumerable<dynamic>> GetProductStatsAsync(string productId)
        {
            var productHistory = await GetProductHistoryAsync(productId);

            return productHistory.Where(p => p.next != null && p.prev != null);
        }

        public async Task<IEnumerable<dynamic>> GetCountryStatsAsync()
        {
            var countryStats = await GetCountryHistoryAsync(null);

            return countryStats.Where(p => p.next != null && p.prev != null);
        }

        public async Task<IEnumerable<dynamic>> GetProductStatsAsync()
        {
            var productStats = await GetProductHistoryAsync(null);

            return productStats.Where(p => p.next != null && p.prev != null);
        }

        public Task<dynamic[]> GetProductsHistoryDepthAsync(IEnumerable<int> products)
        {
            return _orderingContext.OrderItems
                .Where(c => products.Contains(c.ProductId))
                .Select(c => new { c.ProductId, c.Order.OrderDate.Month, c.Order.OrderDate.Year })
                .Distinct()
                .GroupBy(k => k.ProductId, g => new { g.Year, g.Month }, (k, g) => new { ProductId = k, count = g.Count() })
                .Cast<dynamic>()
                .ToArrayAsync();
        }
    }
}