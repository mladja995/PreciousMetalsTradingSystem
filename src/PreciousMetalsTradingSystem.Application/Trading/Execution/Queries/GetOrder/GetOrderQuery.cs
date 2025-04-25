using PreciousMetalsTradingSystem.Application.Trading.Execution.Models;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Queries.GetOrder
{
    public class GetOrderQuery : IRequest<ClientTrade>
    {
        public required string OrderNumber { get; init; }
    }
}
