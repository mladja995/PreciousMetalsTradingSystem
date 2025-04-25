using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class PremiumNotDefinedException : DomainRuleViolationException
    {
        public PremiumNotDefinedException(LocationType location, SideType side) 
            : base($"No premium defined for location {location} and side {side}.")
        {
        }
    }
}
