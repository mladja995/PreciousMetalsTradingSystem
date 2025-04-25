using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class SpotDeferredTradeEntityConfiguration : IEntityTypeConfiguration<SpotDeferredTrade>
    {
        public void Configure(EntityTypeBuilder<SpotDeferredTrade> builder)
        {
            builder.ToTable(TableNames.SpotDeferredTrades);

            builder.Property(sdt => sdt.Id)
               .HasConversion(
                   sdtId => sdtId.Value,
                   value => new SpotDeferredTradeId(value)) 
               .IsRequired();

            builder.Property(ha => ha.HedgingAccountId)
               .HasConversion(
                   haId => haId.Value,
                   value => new HedgingAccountId(value))
               .IsRequired();

            builder.Property(sdt => sdt.TradeConfirmationReference)
                .IsRequired()
                .HasMaxLength(100); 
        }
    }
}
