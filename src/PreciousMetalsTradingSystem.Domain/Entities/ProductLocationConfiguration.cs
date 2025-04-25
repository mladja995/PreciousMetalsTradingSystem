using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class ProductLocationConfiguration 
    {
        public ProductId ProductId { get; private set; }
        public LocationType LocationType { get; private set; }
        public PremiumUnitType PremiumUnitType { get; private set; }
        public Premium BuyPremium { get; private set; } 
        public Premium SellPremium { get; private set; } 
        public bool IsAvailableForBuy { get; private set; }
        public bool IsAvailableForSell { get; private set; }

        public static ProductLocationConfiguration Create(
            LocationType locationType,
            PremiumUnitType premiumUnitType,
            Premium buyPremium,
            Premium sellPremium,
            bool isAvailableForBuy,
            bool isAvailableForSell)
        {
            return new ProductLocationConfiguration
            {
                LocationType = locationType,
                PremiumUnitType = premiumUnitType,
                BuyPremium = buyPremium,
                SellPremium = sellPremium,
                IsAvailableForBuy = isAvailableForBuy,
                IsAvailableForSell = isAvailableForSell
            };
        }

        public void SetProduct(ProductId id)
            => ProductId = id;
    }
}
