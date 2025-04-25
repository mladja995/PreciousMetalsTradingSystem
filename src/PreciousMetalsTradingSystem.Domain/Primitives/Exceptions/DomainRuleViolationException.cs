namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    /// <summary>
    /// Exception for domain rule violations.
    /// Raised when a business rule within the domain layer is broken.
    /// </summary>
    public class DomainRuleViolationException : TradingSystemApplicationException
    {
        public DomainRuleViolationException(string message, string code = "DOMAIN_RULE_VIOLATION")
            : base(message, code)
        {
        }
    }
}
