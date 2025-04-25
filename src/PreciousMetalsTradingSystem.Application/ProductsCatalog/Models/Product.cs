using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Models
{
    public class Product
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required string SKU { get; init; }
        public required decimal WeightInOz { get; init; }
        public required MetalType MetalType { get; init; }
        public required bool IsAvailable { get; init; }
        public IEnumerable<ProductLocationConfiguration> LocationConfigurations { get; init; }

        public static readonly Func<Domain.Entities.Product, Product> Projection =
            product => new Product
            {
                Id = product.Id.Value,
                Name = product.Name,
                SKU = product.SKU.Value,
                IsAvailable = product.IsAvailable,
                MetalType = product.MetalType,
                WeightInOz = product.WeightInOz.Value,
                LocationConfigurations = product.LocationConfigurations
                    .Select(ProductLocationConfiguration.Projection)
            };
    }
}
