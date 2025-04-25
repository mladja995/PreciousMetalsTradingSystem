namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class FinancialAlreadySettledException : DomainRuleViolationException
    {
        public FinancialAlreadySettledException()
            : base("Unable to do a financial settlement, because it has already been settled.")
        {
        }
    }
}
