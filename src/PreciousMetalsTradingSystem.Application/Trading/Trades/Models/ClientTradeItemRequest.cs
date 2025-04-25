namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Models
{
    public class ClientTradeItemRequest : TradeItemRequestBase
    {
        public required decimal SpotPricePerOz { get; init; }
    }
}
