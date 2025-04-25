namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class FinancialTransactionId : BasicId
    {
        public FinancialTransactionId(Guid value) : base(value)
        {
        }

        public static FinancialTransactionId New() => new(Guid.NewGuid());
    }
}
