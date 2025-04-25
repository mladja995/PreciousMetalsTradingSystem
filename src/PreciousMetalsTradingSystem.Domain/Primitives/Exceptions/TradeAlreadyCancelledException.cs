namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class TradeAlreadyCancelledException : DomainRuleViolationException
    {
        public TradeAlreadyCancelledException()
            : base("Unable to do a trade cancellation, because it has already been cancelled.")
        {
        }
    }
}
