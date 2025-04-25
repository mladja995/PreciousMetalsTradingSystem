using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class HedgingAccountEntityConfiguration : IEntityTypeConfiguration<HedgingAccount>
    {
        public void Configure(EntityTypeBuilder<HedgingAccount> builder)
        {
            builder.ToTable(TableNames.HedgingAccounts);

            builder.Property(ha => ha.Id)
               .HasConversion(
                   haId => haId.Value,
                   value => new HedgingAccountId(value)) 
               .IsRequired();

            builder.Property(ha => ha.Name)
                .HasConversion(
                    name => name.Value,
                    value => new HedgingAccountName(value))
                .IsRequired()
                .HasMaxLength(HedgingAccountName.MaxLength);

            builder.Property(ha => ha.Code)
                .HasConversion(
                    code => code.Value,
                    value => new HedgingAccountCode(value))
                .IsRequired()
                .HasMaxLength(HedgingAccountCode.MaxLength);
        }
    }
}
