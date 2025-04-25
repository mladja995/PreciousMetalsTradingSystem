using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Exceptions;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Commands.Update
{
    public class ProductUpdateCommandHandler : IRequestHandler<ProductUpdateCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product, ProductId> _repository;
        private readonly IProductsService _service;

        public ProductUpdateCommandHandler(
            IUnitOfWork unitOfWork,
            IRepository<Product, ProductId> repository,
            IProductsService service)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _service = service;
        }

        public async Task Handle(ProductUpdateCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdOrThrowAsync(
                id: new ProductId(request.Id),
                cancellationToken: cancellationToken,
                includes: p => p.LocationConfigurations);

            // Check if SKU is unique (excluding the current product)
            var sku = new SKU(request.ProductSKU);
            if (!await _service.IsSkuUnique(sku, cancellationToken, product.Id))
            {
                throw new ProductSkuIsNotUniqueException(sku);
            }

            // Prepare location configurations if provided
            var newConfigurations = request.LocationConfigurations?.Select(lc =>
                ProductLocationConfiguration.Create(
                    lc.Location,
                    lc.PremiumUnitType,
                    new Premium(lc.BuyPremium),
                    new Premium(lc.SellPremium),
                    lc.IsAvailableForBuy,
                    lc.IsAvailableForSell)).ToList() ?? [];

            // Update product details and configurations
            product.UpdateProductDetails(
                request.ProductName,
                sku,
                new Weight(request.WeightInOz),
                request.MetalType,
                request.IsAvailableForTrading,
                newConfigurations);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
