using eShopDashboard.Infrastructure.Data.Catalog;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopDashboard.EntityModels.Catalog;
using eShopDashboard.Settings;
using Microsoft.Extensions.Options;

namespace eShopDashboard.Queries
{
    public class CatalogQueries : ICatalogQueries
    {
        private readonly CatalogContext _context;
        private readonly CatalogSettings _settings;

        public CatalogQueries(
            CatalogContext context,
            IOptions<CatalogSettings> options)
        {
            _context = context;
            _settings = options.Value;

        }

        public async Task<CatalogItem> GetCatalogItemById(int catalogItemId)
        {
            return await _context.CatalogItems
                .SingleOrDefaultAsync(ci => ci.Id == catalogItemId);
        }

        public async Task<IEnumerable<dynamic>> GetProductsByDescriptionAsync(string description)
        {
         var itemList = await _context.CatalogItems
                .Where(c => c.Description.Contains(description))
                .Select(ci => new
                {
                    ci.Id,
                    ci.Price,
                    ci.Description,
                    PictureUri = _settings.AzureStorageEnabled 
                        ? _settings.AzurePicBaseUrl + ci.PictureFileName 
                        : string.Format(_settings.LocalPicBaseUrl, ci.Id)
                })
                .ToListAsync();

            return itemList;
        }
    }
}