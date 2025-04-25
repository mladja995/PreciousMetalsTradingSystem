using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class PremiumUnitTypeNotDefinedException : DomainRuleViolationException
    {
        public PremiumUnitTypeNotDefinedException(LocationType location) 
            : base($"Premium unit type is not defined for location {location}.")
        {
        }
    }
}
