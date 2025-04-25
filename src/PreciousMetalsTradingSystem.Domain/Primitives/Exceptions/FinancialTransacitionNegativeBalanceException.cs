namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class FinancialTransacitionNegativeBalanceException : DomainRuleViolationException
    {
        public FinancialTransacitionNegativeBalanceException() 
            : base("Unable to create Financial Transaction, causing balance to go negative.")
        {
        }
    }
}
