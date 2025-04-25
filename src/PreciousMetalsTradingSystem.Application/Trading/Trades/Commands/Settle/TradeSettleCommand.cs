using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Settle
{
    public class TradeSettleCommand : IRequest
    {
        public required Guid Id { get; init; }
    }
}
