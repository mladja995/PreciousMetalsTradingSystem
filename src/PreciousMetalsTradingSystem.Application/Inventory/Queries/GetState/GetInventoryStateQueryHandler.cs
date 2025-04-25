using MediatR;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Application.Inventory.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Application.Common.Extensions;

namespace PreciousMetalsTradingSystem.Application.Inventory.Queries.GetState
{
    public class GetInventoryStateQueryHandler : IRequestHandler<GetInventoryStateQuery, GetInventoryStateQueryResult>
    {
        private readonly IInventoryService _inventoryService;

        public GetInventoryStateQueryHandler(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task<GetInventoryStateQueryResult> Handle(GetInventoryStateQuery request, CancellationToken cancellationToken)
        {
            var allLocationPositions = await _inventoryService.GetRunningPositionsAsync(
                request.Location, request.OnDate, cancellationToken);

            var lastAvailableForTradingPositions = (
                from x in allLocationPositions
                where (int)x.Type == (int)PositionType.AvailableForTrading
                select x
            );

            var lastSettledPositions = (
                from x in allLocationPositions
                where (int)x.Type == (int)PositionType.Settled
                select x
            );

            var positionsDtos = (
                from x in lastAvailableForTradingPositions
                join y in lastSettledPositions on x.ProductId equals y.ProductId into y_left
                from _y in y_left.DefaultIfEmpty()
                select new ProductLocationState
                {
                    Location = x.LocationType,
                    ProductId = x.Product.Id,
                    ProductSKU = x.Product.SKU,
                    ProductName = x.Product.Name,
                    MetalType = x.Product.MetalType,
                    UnitsAvailableForTrading = x.PositionUnits ?? 0,
                    UnitsSettled = _y?.PositionUnits ?? 0,
                }
            )
            .Sort(!string.IsNullOrWhiteSpace(request.Sort) ? request.Sort : "ProductSKU")
            .Skip((request.PageNumber - 1) * request.PageSize!.Value)
            .Take(request.PageSize.Value!)
            .ToList();

            return new GetInventoryStateQueryResult(
                positionsDtos,
                allLocationPositions.Count(x => (int)x.Type == (int)PositionType.AvailableForTrading),
                request.PageNumber,
                request.PageSize.Value!);
        }
    }
}
