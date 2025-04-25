using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class ProductLocationPositionEntityConfiguration : IEntityTypeConfiguration<ProductLocationPosition>
    {
        public void Configure(EntityTypeBuilder<ProductLocationPosition> builder)
        {
            builder.ToTable(TableNames.ProductLocationPositions);

            builder.Property(plp => plp.Id)
               .HasConversion(
                   plpId => plpId.Value,
                   value => new ProductLocationPositionId(value)) 
               .IsRequired();

            // Conversion for ProductId
            builder.Property(plp => plp.ProductId)
                .HasConversion(
                    id => id.Value,
                    value => new ProductId(value))
                .IsRequired();

            // Conversion for nullable TradeId
            builder.Property(ft => ft.TradeId)
                .HasConversion(
                    id => id.Value,  
                    value => new TradeId(value))  
                .IsRequired();

            builder.Property(plp => plp.QuantityUnits)
                .HasConversion(
                    qty => qty.Value,
                    value => new QuantityUnits(value))
                .IsRequired();

            builder.Property(plp => plp.PositionUnits)
                .HasConversion(
                    qty => qty.Value,
                    value => new PositionQuantityUnits(value))
                .IsRequired();
        }
    }
}
