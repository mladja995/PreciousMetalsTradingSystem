using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Trading.Pricing.Services;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Pricing.Queries.GetPrices
{
    public class GetPricesQueryHandler : IRequestHandler<GetPricesQuery, GetPricesQueryResult>
    {
        private readonly IPricingService _service;

        public GetPricesQueryHandler(IPricingService service)
        {
            _service = service;
        }

        public async Task<GetPricesQueryResult> Handle(GetPricesQuery request, CancellationToken cancellationToken)
        {
            var sideType = request.Side is not null ? request.Side.Value.ToSideType() : (SideType?)null;
            var prices = await _service.GetSpotPricesAsync(request.Location, sideType, cancellationToken);
            return new GetPricesQueryResult(prices.ToList());
        }
    }
}
