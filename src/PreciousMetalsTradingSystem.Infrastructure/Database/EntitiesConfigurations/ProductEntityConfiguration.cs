using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class ProductEntityConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable(TableNames.Products);

            builder.Property(p => p.Id)
                .HasConversion(
                    productId => productId.Value, // Convert ProductId to Guid
                    value => new ProductId(value)) // Convert Guid back to ProductId
                .IsRequired();

            // Conversion for SKU value object
            builder.Property(p => p.SKU)
                .HasConversion(
                    sku => sku.Value, // Convert SKU value object to string
                    value => new SKU(value)) // Convert string back to SKU value object
                .IsRequired()
                .HasMaxLength(SKU.MaxLength);

            // Conversion for Weight value object
            builder.Property(p => p.WeightInOz)
                .HasConversion(
                    weight => weight.Value, // Convert Weight value object to decimal
                    value => new Weight(value)) // Convert decimal back to Weight value object
                .HasPrecision(18, 4) // Setting precision for Weight in ounces
                .IsRequired();

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
