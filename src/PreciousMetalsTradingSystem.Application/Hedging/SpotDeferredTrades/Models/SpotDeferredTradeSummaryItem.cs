using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models
{
    public class SpotDeferredTradeSummaryItem
    {
        public required MetalType MetalType { get; init; }
        public required decimal ActualTradedBalance { get; init; }
        public required decimal NetAmount { get; init; } 
        public required DateOnly LastHedgingDate { get; init; }
    }
}
