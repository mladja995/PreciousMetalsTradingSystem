namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Models
{
    public class QuoteItem
    {
        public required string ProductSKU { get; init; }
        public required int QuantityUnits { get; init; } 
        public required decimal PricePerOz { get; init; }
        public required decimal TotalAmount { get; init; }
    }
}
