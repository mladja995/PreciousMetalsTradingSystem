namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class TradeItemId : BasicId
    {
        public TradeItemId(Guid value) : base(value)
        {
        }

        public static TradeItemId New() => new(Guid.NewGuid());
    }
}
