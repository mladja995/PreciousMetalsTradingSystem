using PreciousMetalsTradingSystem.Application.Trading.Pricing.Models;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Pricing.Services
{
    public interface IPricingService
    {
        Task<IEnumerable<ProductPrice>> GetSpotPricesAsync(
            LocationType? location, 
            SideType? side,
            CancellationToken cancellationToken = default);
    }
}
