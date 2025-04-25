using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Models;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Queries.GetSingle
{
    public class GetHedgingItemQuery : IRequest<HedgingItem>
    {
        public required Guid AccountId { get; init; }
        public required Guid HedingItemId { get; init; }
    }
}
