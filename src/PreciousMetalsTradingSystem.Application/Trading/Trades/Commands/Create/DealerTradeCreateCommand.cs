using PreciousMetalsTradingSystem.Application.Common.Locking;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create
{
    public class DealerTradeCreateCommand : IRequest<Guid>, ILockable
    {
        public required DateTime TradeDate { get; init; }
        public required LocationType Location { get; init; }
        public required SideType SideType { get; init; }
        public required bool AutoHedge { get; init; }
        public string? Note { get; init; }
        public required IEnumerable<DealerTradeItemRequest> Items { get; init; }

        public string GetLockKey()
            => CommonLockKeyType.FinancialsAndOrPositionsAffectedLockKey.ToString();
    }
}
