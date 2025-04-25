using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Services
{
    public interface IProductsService
    {
        Task<bool> IsSkuUnique(
            SKU sku,
            CancellationToken cancellationToken = default,
            ProductId? excludeProductId = null);

        /// <summary>
        /// Returns available products for specific location.
        /// Product is considered available on location if there is a configuration set up for that location.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<Product>> GetAvailableProducts(
            LocationType location,
            CancellationToken cancellationToken = default);
    }
}
