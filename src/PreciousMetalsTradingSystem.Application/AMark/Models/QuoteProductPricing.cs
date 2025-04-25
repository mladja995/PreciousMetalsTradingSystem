using Newtonsoft.Json;

namespace PreciousMetalsTradingSystem.Application.AMark.Models
{
    public class QuoteProductPricing
    {
        [JsonProperty("sProductCode")]
        public required string ProductCode { get; init; }

        [JsonProperty("sCommodityDesc")]
        public string CommodityDesc { get; init; }

        [JsonProperty("decSpotPrice")]
        public required decimal SpotPrice { get; init; }

        [JsonProperty("bPremiumIsPercent")]
        public bool PremiumIsPercent { get; init; }

        [JsonProperty("decProductPremium")]
        public decimal ProductPremium { get; init; }

        [JsonProperty("iMinPurchase")]
        public int MinPurchase { get; init; }

        [JsonProperty("decPurchaseIncrement")]
        public decimal PurchaseIncrement { get; init; }

        [JsonProperty("decUnitPrice")]
        public decimal UnitPrice { get; init; }

        [JsonProperty("sTierPrices")]
        public string TierPrices { get; init; }
    }
}
