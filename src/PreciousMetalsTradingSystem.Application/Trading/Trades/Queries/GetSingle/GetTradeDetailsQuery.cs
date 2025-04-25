using PreciousMetalsTradingSystem.Application.Trading.Trades.Models;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Queries.GetSingle
{
    public class GetTradeDetailsQuery : IRequest<TradeDetailsResult>
    {
        public required Guid Id { get; init; }
    }
}
