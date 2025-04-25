using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models
{
    public class SpotDeferredTradeItemResult
    {
        public required DateTime DateUtc { get; init; }
        public required string TradeConfirmationNumber { get; init; }
        public TradeType? TradeType { get; init; }
        public string? TradeReference { get; init; }
        public required SideType SideType { get; init; }
        public required MetalType MetalType { get; init; }
        public required decimal SpotPricePerOz { get; init; }
        public required decimal QuantityOz { get; init; }
        public required decimal TotalAmount { get; init; }
    }
}
