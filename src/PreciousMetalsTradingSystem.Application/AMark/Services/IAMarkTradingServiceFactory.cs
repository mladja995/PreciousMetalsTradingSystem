using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.AMark.Services
{
    public interface IAMarkTradingServiceFactory
    {
        Task<IAMarkTradingService> CreateAsync(
            LocationType locationType,
            CancellationToken cancellationToken = default);
    }
}
