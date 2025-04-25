namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class TradeQuoteItemId : BasicId
    {
        public TradeQuoteItemId(Guid value) : base(value)
        {
        }

        public static TradeQuoteItemId New() => new(Guid.NewGuid());
    }
}
