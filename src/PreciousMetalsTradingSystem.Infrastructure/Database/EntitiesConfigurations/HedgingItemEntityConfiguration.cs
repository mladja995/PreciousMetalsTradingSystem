using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class HedgingItemEntityConfiguration : IEntityTypeConfiguration<HedgingItem>
    {
        public void Configure(EntityTypeBuilder<HedgingItem> builder)
        {
            builder.ToTable(TableNames.HedgingItems);

            builder.Property(hi => hi.Id)
               .HasConversion(
                   hId => hId.Value,
                   value => new HedgingItemId(value)) 
               .IsRequired();

            // Conversion for HedgingAccountId
            builder.Property(hi => hi.HedgingAccountId)
                .HasConversion(
                    id => id.Value,
                    value => new HedgingAccountId(value))
                .IsRequired();

            builder.Property(hi => hi.Amount)
                .HasConversion(
                    amount => amount.Value,
                    value => new Money(value))
                .HasPrecision(18, 2)
                .IsRequired();
        }
    }
}
