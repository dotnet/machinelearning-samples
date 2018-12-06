using eShopDashboard.EntityModels.Catalog;
using Microsoft.EntityFrameworkCore;

namespace eShopDashboard.Infrastructure.Data.Catalog
{
    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions<CatalogContext> options)
            : base(options)
        {
        }

        public DbSet<CatalogItem> CatalogItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CatalogItemConfiguration());
        }
    }
}