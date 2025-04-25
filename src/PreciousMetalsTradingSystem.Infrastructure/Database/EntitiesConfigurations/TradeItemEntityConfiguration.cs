using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class TradeItemEntityConfiguration : IEntityTypeConfiguration<TradeItem>
    {
        public void Configure(EntityTypeBuilder<TradeItem> builder)
        {
            builder.ToTable(TableNames.TradeItems);

            builder.Property(ti => ti.Id)
               .HasConversion(
                   tId => tId.Value,
                   value => new TradeItemId(value))
               .IsRequired();

            builder.Property(ti => ti.TradeId)
                .HasConversion(
                    id => id.Value,
                    value => new TradeId(value))
                .IsRequired();

            builder.Property(ti => ti.TradeQuoteItemId)
                .HasConversion(
                    id => id != null ? id.Value : (Guid?)null,
                    value => value.HasValue ? new TradeQuoteItemId(value.Value) : null)
                .IsRequired(false);

            builder.HasOne<TradeQuoteItem>()
                .WithOne()
                .HasForeignKey<TradeItem>(ti => ti.TradeQuoteItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(plp => plp.ProductId)
                .HasConversion(
                    id => id.Value,
                    value => new ProductId(value))
                .IsRequired();

            builder.Property(ti => ti.SpotPricePerOz)
                .HasConversion(
                    price => price.Value,
                    value => new Money(value))
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(ti => ti.TradePricePerOz)
                .HasConversion(
                    price => price.Value,
                    value => new Money(value))
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(ti => ti.PremiumPerOz)
                .HasConversion(
                    premium => premium.Value,
                    value => new Premium(value))
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(ti => ti.EffectivePricePerOz)
                .HasConversion(
                    price => price.Value,
                    value => new Money(value))
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(ti => ti.TotalRevenue)
                .HasConversion(
                    revenue => revenue.Value,
                    value => new Revenue(value))
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(ft => ft.TotalEffectivePrice)
              .HasConversion(
                  balance => balance.Value,
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
