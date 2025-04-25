namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class NegativeOrZeroAmountException : DomainRuleViolationException
    {
        public NegativeOrZeroAmountException() 
            : base("Amount must be a positive value!")
        {
        }
    }
}
