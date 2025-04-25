using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Premiums.Models
{
    public class ProductPremium
    {
        public required string ProductSKU { get; init; }
        public required string ProductName { get; init; }
        public required LocationType Location { get; init; }
        public required ClientSideType Side { get; init; }
        public required PremiumUnitType PremiumUnitType { get; init; }
        public required decimal PremiumPerOz { get; init; }
    }
}
