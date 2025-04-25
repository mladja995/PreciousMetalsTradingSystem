namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Models
{
    public class ProductInfo
    {
        public required Guid Id { get; init; }
        public required string ProductSKU { get; init; }
        public required string ProductName { get; init; }
    }
}
