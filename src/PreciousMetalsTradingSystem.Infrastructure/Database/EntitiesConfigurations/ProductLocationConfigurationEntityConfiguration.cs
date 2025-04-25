using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class ProductLocationConfigurationEntityConfiguration 
        : IEntityTypeConfiguration<ProductLocationConfiguration>
    {
        public void Configure(EntityTypeBuilder<ProductLocationConfiguration> builder)
        {
            builder.ToTable(TableNames.ProductLocationConfigurations);

            // Configuring composite key with ProductId and LocationType
            builder.HasKey(plc => new { plc.ProductId, plc.LocationType });

            // Conversion for ProductId (value object)
            builder.Property(plc => plc.ProductId)
                .HasConversion(
                    productId => productId.Value,  // Convert ProductId to Guid
                    value => new ProductId(value)) // Convert Guid back to ProductId
                .IsRequired();

            // Conversion for BuyPremium value object
            builder.Property(plc => plc.BuyPremium)
                .HasConversion(
                    premium => premium.Value,  // Convert Premium value object to decimal
                    value => new Premium(value))  // Convert decimal back to Premium value object
                .HasPrecision(18, 2)
                .IsRequired();

            // Conversion for SellPremium value object
            builder.Property(plc => plc.SellPremium)
                .HasConversion(
                    premium => premium.Value,  // Convert Premium value object to decimal
                    value => new Premium(value))  // Convert decimal back to Premium value object
                .HasPrecision(18, 2);
        }
    }
}
