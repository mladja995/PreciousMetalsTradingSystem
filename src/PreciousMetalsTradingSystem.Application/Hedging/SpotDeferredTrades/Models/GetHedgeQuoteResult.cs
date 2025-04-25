using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models
{
    public class GetHedgeQuoteResult
    {
        public required string QuoteKey { get; init; }
        public required Dictionary<MetalType, decimal> SpotPricesPerOz { get; init; }
    }
}
