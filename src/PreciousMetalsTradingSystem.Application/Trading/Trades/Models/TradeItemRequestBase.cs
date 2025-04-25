namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Models
{
    public class TradeItemRequestBase
    {
        public required Guid ProductId { get; init; }
        public required int UnitQuantity { get; init; }
    }
}
