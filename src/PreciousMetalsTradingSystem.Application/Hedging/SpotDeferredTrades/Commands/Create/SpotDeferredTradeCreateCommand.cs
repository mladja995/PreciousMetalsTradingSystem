using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Commands.Create
{
    public class SpotDeferredTradeCreateCommand : IRequest<Guid>
    {
        [OpenApiExclude]
        public Guid AccountId { get; set; }
        public required DateTime Date { get; init; }
        public required string TradeConfirmationNumber { get; init; }
        public required SideType SideType { get; init; }
        public required IEnumerable<SpotDeferredTradeItem> Items { get; init; }
    }
}
