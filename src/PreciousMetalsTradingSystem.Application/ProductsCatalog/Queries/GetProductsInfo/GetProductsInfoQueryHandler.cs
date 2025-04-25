using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductEntities = PreciousMetalsTradingSystem.Domain.Entities;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetProductsInfo
{
    public class GetProductsInfoQueryHandler : IRequestHandler<GetProductsInfoQuery, GetProductsInfoQueryResult>
    {
        private readonly IRepository<ProductEntities.Product, ProductId> _repository;

        public GetProductsInfoQueryHandler(IRepository<ProductEntities.Product, ProductId> repository)
        {
            _repository = repository;
        }

        public async Task<GetProductsInfoQueryResult> Handle(GetProductsInfoQuery request, CancellationToken cancellationToken)
        {
            return new GetProductsInfoQueryResult(
                await _repository.StartQuery(true)
                .OrderBy(x => x.SKU)
                .Select(x => new ProductInfo
                {
                    Id = x.Id,
                    ProductSKU = x.SKU,
                    ProductName = x.Name,
                }).ToListAsync(cancellationToken));
        }
    }
}
