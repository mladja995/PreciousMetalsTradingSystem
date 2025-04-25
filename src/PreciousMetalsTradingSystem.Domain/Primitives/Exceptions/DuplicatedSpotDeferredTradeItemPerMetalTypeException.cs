using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class DuplicatedSpotDeferredTradeItemPerMetalTypeException : DomainRuleViolationException
    {
        public DuplicatedSpotDeferredTradeItemPerMetalTypeException(MetalType metal)
            : base($"Item for metal type '{metal}' already exists.")
        {
        }
    }
}
