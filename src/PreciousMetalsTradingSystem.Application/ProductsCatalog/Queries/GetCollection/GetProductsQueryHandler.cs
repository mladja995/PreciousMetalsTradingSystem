using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using ProductEntities = PreciousMetalsTradingSystem.Domain.Entities;
using ProductCatalogModels = PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;
namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetCollection
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsQueryResult>
    {
        private readonly IRepository<ProductEntities.Product, ProductId> _repository;

        public GetProductsQueryHandler(IRepository<ProductEntities.Product, ProductId> repository) 
        {
            _repository = repository;
        }

        public async Task<GetProductsQueryResult> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _repository.GetAllAsync(
                readOnly: true,
                pageNumber: request.PageNumber, 
                pageSize: request.PageSize, 
                sort: !string.IsNullOrWhiteSpace(request.Sort) ? request.Sort : "SKU", 
                cancellationToken: cancellationToken,
                includes: p => p.LocationConfigurations);

            return new GetProductsQueryResult(
                items.Select(ProductCatalogModels.Product.Projection).ToList(), 
                totalCount!.Value, 
                request.PageNumber, 
                request.PageSize!.Value);
        }
    }
}
