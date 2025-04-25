using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;
using PreciousMetalsTradingSystem.Application.Common.Locking;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Cancel
{
    public class CancelTradeCommand : IRequest, ILockable
    {
        // HACK: string.Empty -> Because of model binding error for string type
        [OpenApiExclude]
        public string TradeNumber { get; set; } = string.Empty;
        public required bool AutoHedge { get; init; }

        public string GetLockKey()
            => CommonLockKeyType.FinancialsAndOrPositionsAffectedLockKey.ToString();
    }
}
