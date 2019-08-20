using eShopDashboard.EntityModels.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShopDashboard.Infrastructure.Data.Catalog
{
    public class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.ToTable("CatalogItems", schema: "Catalog");

            builder.Property(ci => ci.Id)
                .IsRequired(true)
                .ValueGeneratedNever();

            builder.Property(ci => ci.Name)
                .IsRequired(true)
                .HasMaxLength(50);

            builder.Property(ci => ci.Price)
                .IsRequired(true);

            builder.Property(ci => ci.PictureUri)
                .IsRequired(false);

            builder.Property(ci => ci.TagsJson)
                .IsRequired(false)
                .HasMaxLength(4000);
        }
    }
}