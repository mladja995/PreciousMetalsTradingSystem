using MediatR;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using DomainEntities = PreciousMetalsTradingSystem.Domain.Entities;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetSingle
{
    public class GetProductQueryHandler : IRequestHandler<GetProductQuery, Product>
    {
        private readonly IRepository<DomainEntities.Product, ProductId> _repository;

        public GetProductQueryHandler(IRepository<Domain.Entities.Product, ProductId> repository)
        {
            _repository = repository;
        }

        public async Task<Product> Handle(GetProductQuery request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdOrThrowAsync(
                id: new ProductId(request.Id),
                cancellationToken: cancellationToken,
                includes: p => p.LocationConfigurations);

            return Product.Projection(product);
        }
    }
}
