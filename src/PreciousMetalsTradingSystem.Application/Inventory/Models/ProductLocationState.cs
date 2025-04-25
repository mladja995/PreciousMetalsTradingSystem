using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Inventory.Models
{
    /// <summary>
    /// Represents the current state of a specific product at a given location, 
    /// detailing units available for trading and units that have been settled.
    /// </summary>
    public class ProductLocationState
    {
        public required LocationType Location { get; init; }
        public required Guid ProductId { get; init; }
        public required string ProductSKU { get; init; }
        public required string ProductName { get; init; }
        public required MetalType MetalType { get; init; }
        public required int UnitsAvailableForTrading { get; init; }
        public required int UnitsSettled { get; init; }
    }
}
