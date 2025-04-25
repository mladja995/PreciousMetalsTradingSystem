using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class TradeEntityConfiguration : IEntityTypeConfiguration<Trade>
    {
        public void Configure(EntityTypeBuilder<Trade> builder)
        {
            builder.ToTable(TableNames.Trades);

            builder.Property(t => t.Id)
               .HasConversion(
                   tId => tId.Value,
                   value => new TradeId(value)) 
               .IsRequired();

            builder.Property(ot => ot.OffsetTradeId)
               .HasConversion(
                   otId => otId != null ? otId.Value : (Guid?)null,
                   value => value.HasValue ? new TradeId(value.Value) : null);

            builder.HasOne(t => t.OffsetTrade)
                .WithOne()
                .HasForeignKey<Trade>(t => t.OffsetTradeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.TradeQuoteId)
                .HasConversion(
                    id => id != null ? id.Value : (Guid?)null,
                    value => value.HasValue ? new TradeQuoteId(value.Value) : null)
                .IsRequired(false);

            builder.HasOne<TradeQuote>()
               .WithOne()
               .HasForeignKey<Trade>(ti => ti.TradeQuoteId)
               .OnDelete(DeleteBehavior.Restrict); 

            builder.Property(tq => tq.SpotDeferredTradeId)
                .HasConversion(
                    id => id != null ? id.Value : (Guid?)null,
                    value => value.HasValue ? new SpotDeferredTradeId(value.Value) : null)
                .IsRequired(false);

            builder.HasOne<SpotDeferredTrade>()
                .WithMany(s => s.Trades)
                .HasForeignKey(t => t.SpotDeferredTradeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.TradeNumber)
                .IsRequired()
                .HasMaxLength(100); 

            builder.Ignore(x => x.IsCancellationAllowed);
        }
    }
}
