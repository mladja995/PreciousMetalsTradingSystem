using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Premiums.Queries
{
    public class GetPremiumsQuery : IRequest<GetPremiumsQueryResult>
    {
        public LocationType? Location {  get; init; }
        public ClientSideType? ClientSide { get; init; }
    }
}
