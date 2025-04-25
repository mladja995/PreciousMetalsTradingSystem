using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class DuplicatedProductLocationConfigurationException : DomainRuleViolationException
    {
        public DuplicatedProductLocationConfigurationException(LocationType location)
            : base($"The product configuration for the location '{location}' already exists.")
        {
        }
    }
}
