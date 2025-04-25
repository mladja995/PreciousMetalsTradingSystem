using Newtonsoft.Json;

namespace PreciousMetalsTradingSystem.Application.AMark.Models
{
    public class QuoteResponse
    {
        [JsonProperty("sQuoteKey")]
        public required string QuoteKey { get; init; }

        [JsonProperty("decSmallOrderCharge")]
        public decimal SmallOrderCharge { get; init; }

        [JsonProperty("decShippingCost")]
        public decimal ShippingCost { get; init; }

        [JsonProperty("QuoteProductsPricingList")]
        public required List<QuoteProductPricing> QuoteProductsPricingList { get; init; } = [];
    }
}
