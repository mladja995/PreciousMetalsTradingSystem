using PreciousMetalsTradingSystem.Domain.Entities;

namespace PreciousMetalsTradingSystem.Domain.DomainServices
{
    public interface ITradeCancellationService
    {
        Trade CancelWithOffset(Trade tradeToBeCancelled);
    }
}
