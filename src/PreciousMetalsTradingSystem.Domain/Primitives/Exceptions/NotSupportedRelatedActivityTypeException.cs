using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class NotSupportedRelatedActivityTypeException : DomainRuleViolationException
    {
        public NotSupportedRelatedActivityTypeException(ActivityType activityType) 
            : base($"The type {activityType} of related activity is not supported for Financial Transaction entity.")
        {
        }
    }
}
