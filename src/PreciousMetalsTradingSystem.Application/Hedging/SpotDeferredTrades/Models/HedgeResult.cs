using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models
{
    public class HedgeResult
    {
        public required string QuoteKey { get; init; }
        public required string TradeConfirmationNumber { get; init; }
        public required Dictionary<MetalType, decimal> SpotPricesPerOz { get; init; }
    }
}
