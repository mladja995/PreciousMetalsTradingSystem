using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Services
{
    public class ProductsService : IProductsService
    {
        private readonly IRepository<Product, ProductId> _repository;

        public ProductsService(IRepository<Product, ProductId> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Product>> GetAvailableProducts(
            LocationType location, 
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _repository.GetAllAsync(
                filter: p => p.LocationConfigurations.Any(lc => lc.LocationType == location),
                readOnly: true,
                sort: "SKU",
                cancellationToken: cancellationToken,
                includes: p => p.LocationConfigurations);

            return items;
        }

        public async Task<bool> IsSkuUnique(
            SKU sku, 
            CancellationToken cancellationToken,
            ProductId? excludeProductId = null)
        {
            var productsWithSameSKU = await _repository
                .GetAllAsync(p => p.SKU.Equals(sku) && (excludeProductId == null || p.Id != excludeProductId),
                readOnly: true,
                cancellationToken: cancellationToken);

            return !productsWithSameSKU.items.Any();
        }
    }
}
