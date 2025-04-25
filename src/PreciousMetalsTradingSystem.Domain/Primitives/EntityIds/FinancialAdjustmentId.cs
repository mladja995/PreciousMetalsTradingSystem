namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class FinancialAdjustmentId : BasicId
    {
        public FinancialAdjustmentId(Guid value) : base(value) {}
        public static FinancialAdjustmentId New() => new(Guid.NewGuid());
    }
}
