using PreciousMetalsTradingSystem.Application.Financials.Models;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Financials.Queries.GetCurrentBalance
{
    public class GetCurrentBalanceQuery : IRequest<Balance>
    {
    }
}
