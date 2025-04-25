using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public class ProductLocationConfigurationId : ValueObject
    {
        public ProductId ProductId { get; }
        public LocationType LocationType { get; }

        public ProductLocationConfigurationId(ProductId productId, LocationType location)
        {
            ArgumentNullException.ThrowIfNull(productId);

            ProductId = productId;
            LocationType = location;
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return ProductId;
            yield return LocationType;
        }

        public override string ToString() => $"{ProductId}-{LocationType}";
    }
}
