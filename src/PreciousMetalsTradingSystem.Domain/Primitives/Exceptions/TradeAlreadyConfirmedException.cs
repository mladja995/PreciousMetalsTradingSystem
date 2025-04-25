namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class TradeAlreadyConfirmedException : DomainRuleViolationException
    {
        public TradeAlreadyConfirmedException() 
            : base("Unable to do a trade confirmation, because it has already been confirmed.") 
        { 
        }
    }
}
