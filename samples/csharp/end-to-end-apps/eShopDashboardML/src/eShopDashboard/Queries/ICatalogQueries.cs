using eShopDashboard.EntityModels.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShopDashboard.Queries
{
    public interface ICatalogQueries
    {
        Task<CatalogItem> GetCatalogItemById(int catalogItemId);

        Task<IEnumerable<dynamic>> GetProductsByDescriptionAsync(string description);
    }
}