namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class PositionAlreadySettledException : DomainRuleViolationException
    {
        public PositionAlreadySettledException()
            : base("Unable to do a position settlement, because it has already been settled.")
        {
        }
    }
}
