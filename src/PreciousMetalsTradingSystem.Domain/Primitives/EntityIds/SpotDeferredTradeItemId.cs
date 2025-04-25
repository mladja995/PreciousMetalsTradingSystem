namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class SpotDeferredTradeItemId : BasicId
    {
        public SpotDeferredTradeItemId(Guid value) : base(value)
        {
        }

        public static SpotDeferredTradeItemId New() => new(Guid.NewGuid());
    }
}
