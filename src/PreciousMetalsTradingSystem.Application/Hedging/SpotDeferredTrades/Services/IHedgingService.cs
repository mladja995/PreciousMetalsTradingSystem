using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Application.Heding.SpotDeferredTrades.Services
{
    public interface IHedgingService
    {
        Task<HedgeResult> HedgeAsync(
            Dictionary<MetalType, QuantityOunces> quantityItems, 
            SideType side, 
            LocationType locationType,
            string referenceNumber,
            CancellationToken cancellationToken = default);

        Task<GetHedgeQuoteResult> GetHedgeQuoteAsync(
            Dictionary<MetalType, QuantityOunces> quantityItems,
            SideType side,
            LocationType locationType,
            CancellationToken cancellationToken = default);

        Task<ExecuteHedgeQuoteResult> ExecuteHedgeQuoteAsync(
            LocationType locationType,
            string quoteKey,
            string referenceNumber,
            CancellationToken cancellationToken = default);
    }
}
