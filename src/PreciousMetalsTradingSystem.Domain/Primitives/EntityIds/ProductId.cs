namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class ProductId : BasicId
    {
        public ProductId(Guid value) : base(value) { }

        public static ProductId New() => new(Guid.NewGuid());
    }
}
