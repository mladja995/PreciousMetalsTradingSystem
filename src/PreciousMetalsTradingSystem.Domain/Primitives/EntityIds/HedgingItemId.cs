namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class HedgingItemId : BasicId
    {
        public HedgingItemId(Guid value) : base(value)
        {
        }

        public static HedgingItemId New() => new(Guid.NewGuid());
    }
}
