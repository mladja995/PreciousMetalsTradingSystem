namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class TradeId : BasicId
    {
        public TradeId(Guid value) : base(value)
        {
        }

        public static TradeId New() => new(Guid.NewGuid());
    }
}
