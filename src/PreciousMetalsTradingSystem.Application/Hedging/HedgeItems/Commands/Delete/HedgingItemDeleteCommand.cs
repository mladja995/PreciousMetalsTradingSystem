using MediatR;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Delete
{
    public class HedgingItemDeleteCommand : IRequest
    {
        public required Guid AccountId { get; init; }
        public required Guid Id { get; init; }
    }
}
