using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Premiums.Models;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Premiums.Queries
{
    public class GetPremiumsQueryHandler : IRequestHandler<GetPremiumsQuery, GetPremiumsQueryResult>
    {
        private readonly IRepository<Product, ProductId> _repository;

        public GetPremiumsQueryHandler(IRepository<Product, ProductId> repository) 
        { 
            _repository = repository; }


        public async Task<GetPremiumsQueryResult> Handle(GetPremiumsQuery request, CancellationToken cancellationToken)
        {
            var premiums = new List<ProductPremium>();
            var products = await GetAllProducts(cancellationToken);
            var requestedLocations = GetRequestedLocations(request.Location);

            foreach (var location in requestedLocations)
            {
                var sides = GetRequestedSides(request.ClientSide);
                foreach (var side in sides)
                {
                    premiums.AddRange(
                        BuildPremiums(
                            location,
                            side,
                            products));
                }
            }

            return new GetPremiumsQueryResult(premiums
                .OrderBy(x => x.ProductSKU)
                .ThenBy(x => x.Location)
                .ThenBy(x => x.Side).ToList()
                );
        }

        private async Task<IEnumerable<Product>> GetAllProducts(CancellationToken cancellationToken = default)
        {
            return (await _repository.GetAllAsync(
                readOnly: true,
                sort: "SKU",
                cancellationToken: cancellationToken,
                includes: x => x.LocationConfigurations)).items;
        }

        private static IEnumerable<LocationType> GetRequestedLocations(LocationType? location)
            => location.HasValue ?
                [location.Value]
                : Enum.GetValues(typeof(LocationType)).Cast<LocationType>();

        private static IEnumerable<SideType> GetRequestedSides(ClientSideType? sideType)
          => sideType.HasValue ?
              [sideType.Value.ToSideType()]
              : Enum.GetValues(typeof(SideType)).Cast<SideType>();

        private static IEnumerable<ProductPremium> BuildPremiums(
            LocationType location,
            SideType side,
            IEnumerable<Product> products)
        {
            return products
                .Where(x => x.LocationConfigurations.Any(y => y.LocationType == location))
                .Select(x => new ProductPremium
                {
                    ProductSKU = x.SKU,
                    ProductName = x.Name,
                    Location = location,
                    Side = side.ToClientSideType(),
                    PremiumUnitType = x.GetPremiumUnitType(location)!.Value,
                    PremiumPerOz = x.GetPremium(location, side)!.Value,
                });
        }
    }
}
