using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models
{
    public class SpotDeferredTradeItem
    {
        public required MetalType MetalType { get; init; }
        public required decimal SpotPricePerOz { get; init; }
        public required decimal QuantityOz { get; init; }
    }
}
