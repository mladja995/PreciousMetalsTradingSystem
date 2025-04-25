using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class UnsupportedPremiumUnitTypeException : DomainRuleViolationException
    {
        public UnsupportedPremiumUnitTypeException(PremiumUnitType premiumUnitType) 
            : base($"Unsupported premium unit type '{premiumUnitType}' for price calculation.")
        {
        }
    }
}
