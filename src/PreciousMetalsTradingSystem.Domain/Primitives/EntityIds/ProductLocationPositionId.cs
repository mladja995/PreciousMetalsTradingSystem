namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class ProductLocationPositionId : BasicId
    {
        public ProductLocationPositionId(Guid value) : base(value)
        {
        }

        public static ProductLocationPositionId New() => new(Guid.NewGuid());
    }
}
