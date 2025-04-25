using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Pricing.Queries.GetPrices
{
    public class GetPricesQuery : IRequest<GetPricesQueryResult>
    {
        public LocationType? Location { get; init; }
        public ClientSideType? Side { get; init; }
    }
}
