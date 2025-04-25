using Newtonsoft.Json;

namespace PreciousMetalsTradingSystem.Application.AMark.Models
{
    public class TradeResponse
    {
        [JsonProperty("sQuoteKey")]
        public required string QuoteKey { get; init; }

        [JsonProperty("sTicketNumber")]
        public required string TicketNumber { get; init; }
    }
}
