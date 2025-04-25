using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class LocationHedgingAccountConfiguration : AggregateRoot<LocationHedgingAccountConfigurationId>
    {
        public HedgingAccountId HedgingAccountId { get; private set; }
        public virtual HedgingAccount HedgingAccount { get; private set; }

        public static LocationHedgingAccountConfiguration Create(
            LocationType location,
            HedgingAccountId hedgingAccountId)
        {
            hedgingAccountId.ThrowIfNull();

            return new LocationHedgingAccountConfiguration
            {
                Id = new LocationHedgingAccountConfigurationId(location),
                HedgingAccountId = hedgingAccountId
            };

        }
    }
}
