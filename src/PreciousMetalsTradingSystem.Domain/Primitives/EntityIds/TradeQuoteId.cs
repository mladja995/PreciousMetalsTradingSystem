namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class TradeQuoteId : BasicId
    {
        public TradeQuoteId(Guid value) : base(value)
        {
        }

        public static TradeQuoteId New() => new(Guid.NewGuid());
    }
}
