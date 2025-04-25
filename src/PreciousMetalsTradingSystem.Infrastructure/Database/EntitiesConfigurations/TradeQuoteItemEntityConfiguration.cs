using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class TradeQuoteItemEntityConfiguration : IEntityTypeConfiguration<TradeQuoteItem>
    {
        public void Configure(EntityTypeBuilder<TradeQuoteItem> builder)
        {
            builder.ToTable(TableNames.TradeQuoteItems);

            builder.Property(tqi => tqi.Id)
               .HasConversion(
                   tqId => tqId.Value,
                   value => new TradeQuoteItemId(value))
               .IsRequired();

            builder.Property(tq => tq.TradeQuoteId)
               .HasConversion(
                   tqId => tqId.Value,
                   value => new TradeQuoteId(value))
               .IsRequired();

            builder.Property(plp => plp.ProductId)
                .HasConversion(
                    id => id.Value,
                    value => new ProductId(value))
                .IsRequired();

            builder.Property(tqi => tqi.SpotPricePerOz)
                .HasConversion(
                    price => price.Value,
                    value => new Money(value))
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(tqi => tqi.PremiumPricePerOz)
                .HasConversion(
                    premium => premium.Value,
                    value => new Premium(value))
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(tqi => tqi.EffectivePricePerOz)
                .HasConversion(
                    price => price.Value,
                    value => new Money(value))
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(plp => plp.QuantityUnits)
                .HasConversion(
                    qty => qty.Value,
                    value => new QuantityUnits(value))
                .IsRequired();
        }
    }
}
