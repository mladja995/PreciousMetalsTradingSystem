using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Pricing.Models
{
    public class ProductPrice
    {
        public required string ProductSKU { get; init; }
        public required string ProductName { get; init; }
        public required LocationType Location { get; init; }
        public required ClientSideType Side { get; init; }
        public required DateTime TimestampUtc { get; init; }
        public required bool IsAvaiable { get; init; }
        public required decimal WeightInOz { get; init; }
        public required decimal SpotPricePerOz { get; init; }
        public required PremiumUnitType PremiumUnitType { get; init; }
        public required decimal PremiumPerOz { get; init; }
    }
}
