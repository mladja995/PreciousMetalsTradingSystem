namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Models
{
    public class DealerTradeItemRequest : TradeItemRequestBase
    {
        public required decimal DealerPricePerOz { get; init; }
        public decimal? SpotPricePerOz { get; init; }
    }
}
