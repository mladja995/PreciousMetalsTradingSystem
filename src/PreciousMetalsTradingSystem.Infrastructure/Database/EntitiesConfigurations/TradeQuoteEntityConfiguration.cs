using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class TradeQuoteEntityConfiguration : IEntityTypeConfiguration<TradeQuote>
    {
        public void Configure(EntityTypeBuilder<TradeQuote> builder)
        {
            builder.ToTable(TableNames.TradeQuotes);

            builder.Property(tq => tq.Id)
               .HasConversion(
                   tqId => tqId.Value,
                   value => new TradeQuoteId(value))
               .IsRequired();

            builder.Property(tq => tq.DealerQuoteId)
                .IsRequired()
                .HasMaxLength(255);  
        }
    }
}
