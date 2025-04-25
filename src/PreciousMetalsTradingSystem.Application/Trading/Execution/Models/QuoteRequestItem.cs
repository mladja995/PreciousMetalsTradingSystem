using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Models
{
    public class QuoteRequestItem
    {
        public required string ProductSKU { get; init; }
        public required int QuantityUnits { get; init; }
    }
}
