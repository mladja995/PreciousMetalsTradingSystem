using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class LocationHedgingAccountConfigurationEntityConfiguration : IEntityTypeConfiguration<LocationHedgingAccountConfiguration>
    {
        public void Configure(EntityTypeBuilder<LocationHedgingAccountConfiguration> builder)
        {
            builder.ToTable(TableNames.LocationHedgingAccountConfigurations);

            builder.Property(lhac => lhac.Id)
                .HasConversion(
                    lhacId => (int)lhacId.LocationType,
                    value => new LocationHedgingAccountConfigurationId((LocationType)value))
                .IsRequired();

            builder.Property(lhac => lhac.HedgingAccountId)
                .HasConversion(
                    id => id.Value,
                    value => new HedgingAccountId(value))
                .IsRequired();
        }
    }
}
