using MediatR;

namespace PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetDetails
{
    public class GetHedgingAccountDetailsQuery : IRequest<GetHedgingAccountDetailsQueryResult>
    {
        public required Guid Id { get; init; }
    }
}
