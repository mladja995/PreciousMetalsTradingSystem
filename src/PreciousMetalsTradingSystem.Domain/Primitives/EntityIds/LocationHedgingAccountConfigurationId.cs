using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;

namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class LocationHedgingAccountConfigurationId : ValueObject, IEntityId
    {
        public LocationType LocationType { get; }
        public LocationHedgingAccountConfigurationId(LocationType location) 
        {
            LocationType = location;
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return LocationType;
        }
    }
}
