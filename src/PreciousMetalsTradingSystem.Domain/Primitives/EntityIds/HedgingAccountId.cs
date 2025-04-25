namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class HedgingAccountId : BasicId
    {
        public HedgingAccountId(Guid value) : base(value)
        {
        }

        public static HedgingAccountId New() => new(Guid.NewGuid());
    }
}
