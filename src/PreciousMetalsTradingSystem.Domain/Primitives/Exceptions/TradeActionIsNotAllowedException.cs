namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class TradeActionIsNotAllowedException : DomainRuleViolationException
    {
        public TradeActionIsNotAllowedException(string message) : base(message) { } 
    }
}
