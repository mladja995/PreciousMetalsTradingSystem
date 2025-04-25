using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Exceptions;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Commands.Create
{
    public class ProductCreateCommandHandler : IRequestHandler<ProductCreateCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product, ProductId> _repository;
        private readonly IProductsService _service;

        public ProductCreateCommandHandler(
            IUnitOfWork unitOfWork, 
            IRepository<Product, ProductId> repository,
            IProductsService service)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _service = service;
        }

        public async Task<Guid> Handle(ProductCreateCommand request, CancellationToken cancellationToken)
        {
            var sku = new SKU(request.ProductSKU);
            if (!await _service.IsSkuUnique(sku, cancellationToken))
            {
                throw new ProductSkuIsNotUniqueException(sku);
            }

            var configurations = new List<ProductLocationConfiguration>();

            var newProduct = Product.Create(
                    request.ProductName,
                    sku,
                    new Weight(request.WeightInOz),
                    request.MetalType,
                    request.IsAvailableForTrading,
                    request.LocationConfigurations?.Select(x => 
                        ProductLocationConfiguration.Create(
                            x.Location,
                            x.PremiumUnitType,
                            new Premium(x.BuyPremium),
                            new Premium(x.SellPremium),
                            x.IsAvailableForBuy,
                            x.IsAvailableForSell)));

            await _repository.AddAsync(newProduct, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return newProduct.Id.Value;
        }
    }
}
