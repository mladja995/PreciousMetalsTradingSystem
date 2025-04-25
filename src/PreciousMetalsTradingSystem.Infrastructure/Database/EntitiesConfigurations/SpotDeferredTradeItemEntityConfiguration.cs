using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class SpotDeferredTradeItemEntityConfiguration : IEntityTypeConfiguration<SpotDeferredTradeItem>
    {
        public void Configure(EntityTypeBuilder<SpotDeferredTradeItem> builder)
        {
            builder.ToTable(TableNames.SpotDeferredTradeItems);

            builder.Property(sdti => sdti.Id)
               .HasConversion(
                   sdtId => sdtId.Value,
                   value => new SpotDeferredTradeItemId(value)) 
               .IsRequired();

            // Conversion for SpotDeferredTradeId
            builder.Property(sdi => sdi.SpotDeferredTradeId)
                .HasConversion(
                    id => id.Value,
                    value => new SpotDeferredTradeId(value))
                .IsRequired();

            builder.Property(sdi => sdi.PricePerOz)
                .HasConversion(
                    price => price.Value,
                    value => new Money(value))
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(sdi => sdi.QuantityOz)
                .HasConversion(
                    qty => qty.Value,
                    value => new QuantityOunces(value))
                .HasPrecision(18, 4)
                .IsRequired();

            builder.Property(sdi => sdi.TotalAmount)
                .HasConversion(
                    total => total.Value,
                    value => new Money(value))
                .HasPrecision(18, 2)
                .IsRequired();
        }
    }
}
