using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Models
{
    public class ProductLocationConfiguration
    {
        public required LocationType Location { get; init; }
        public required PremiumUnitType PremiumUnitType { get; init; }
        public required decimal BuyPremium { get; init; }
        public required decimal SellPremium { get; init; }
        public required bool IsAvailableForBuy { get; init; }
        public required bool IsAvailableForSell { get; init; }

        public static readonly Func<Domain.Entities.ProductLocationConfiguration, ProductLocationConfiguration> Projection =
            config => new ProductLocationConfiguration
            {
                Location = config.LocationType,
                BuyPremium = config.BuyPremium.Value,
                SellPremium = config.SellPremium.Value,
                IsAvailableForBuy = config.IsAvailableForBuy,
                IsAvailableForSell = config.IsAvailableForSell,
                PremiumUnitType = config.PremiumUnitType
            };
    }
}