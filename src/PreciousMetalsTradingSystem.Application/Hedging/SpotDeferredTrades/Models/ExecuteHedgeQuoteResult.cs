namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models
{
    public class ExecuteHedgeQuoteResult
    {
        public required string QuoteKey { get; init; }
        public required string TradeConfirmationNumber { get; init; }
    }
}
