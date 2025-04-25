using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Inventory.Models;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainEntities = PreciousMetalsTradingSystem.Domain.Entities;
namespace PreciousMetalsTradingSystem.Application.Inventory.Queries.GetPositionsHistory
{
    public class GetInventoryPositionsHistoryQueryHandler : IRequestHandler<GetInventoryPositionsHistoryQuery, GetInventoryPositionsHistoryQueryResult>
    {
        private readonly IRepository<DomainEntities.ProductLocationPosition, ProductLocationPositionId> _repository;
        
        public GetInventoryPositionsHistoryQueryHandler(
            IRepository<DomainEntities.ProductLocationPosition, ProductLocationPositionId> repository)
        {
            _repository = repository;
        }
        
        public async Task<GetInventoryPositionsHistoryQueryResult> Handle(GetInventoryPositionsHistoryQuery request, CancellationToken cancellationToken)
        {
            // Start the query with necessary includes
            var inventoryPositionsHistoryQuery = _repository.StartQuery(readOnly: true)
                .Include(x => x.Product) 
                .Include(x => x.Trade)  
                .Where(x =>
                    x.Product.SKU.Equals(new SKU(request.ProductSKU!)) &&
                    x.LocationType == request.Location &&
                    x.Type == request.PositionType)
                .Select(ProductLocationPositionHistory.Projection);

            // Fetch the total count
            int totalCount = await inventoryPositionsHistoryQuery.CountAsync(cancellationToken);

            // Apply pagination and fetch the result
            var itemsDto = await inventoryPositionsHistoryQuery
                .Sort(!string.IsNullOrWhiteSpace(request.Sort) ? request.Sort : "-TimestampUtc") // TODO: Refactor default sort param
                .Skip((request.PageNumber - 1) * request.PageSize!.Value)
                .Take(request.PageSize.Value!)
                .ToListAsync(cancellationToken);

            // Return the result
            return new GetInventoryPositionsHistoryQueryResult(
                itemsDto,
                totalCount,
                request.PageNumber,
                request.PageSize.Value!
            );
        }
    }
}
