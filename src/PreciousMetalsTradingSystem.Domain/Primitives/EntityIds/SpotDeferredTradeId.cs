namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class SpotDeferredTradeId : BasicId
    {
        public SpotDeferredTradeId(Guid value) : base(value)
        {
        }

        public static SpotDeferredTradeId New() => new(Guid.NewGuid());
    }
}
