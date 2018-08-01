using eShopDashboard.Infrastructure.Data.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Scripts.Cli
{
    public class CatalogContextDesignTimeFactory : IDesignTimeDbContextFactory<CatalogContext>
    {
        public CatalogContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CatalogContext>();

            builder.UseSqlServer("x");

            return new CatalogContext(builder.Options);
        }
    }
}