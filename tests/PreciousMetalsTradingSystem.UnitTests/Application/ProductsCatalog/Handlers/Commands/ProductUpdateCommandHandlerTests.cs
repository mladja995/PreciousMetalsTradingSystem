using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Commands.Update;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Exceptions;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;
using System.Linq.Expressions;
using ProductModel = PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;

namespace PreciousMetalsTradingSystem.UnitTests.Application.ProductsCatalog.Handlers.Commands
{
    public class ProductUpdateCommandHandlerTests
    {
        private readonly Mock<IRepository<Product, ProductId>> _productRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProductsService> _productsServiceMock;
        private readonly ProductUpdateCommandHandler _handler;

        public ProductUpdateCommandHandlerTests()
        {
            _productRepositoryMock = new Mock<IRepository<Product, ProductId>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _productsServiceMock = new Mock<IProductsService>();
            _handler = new ProductUpdateCommandHandler(_unitOfWorkMock.Object, _productRepositoryMock.Object, _productsServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldSucceed_WhenProductIsUpdatedSuccessfully()
        {
            // Arrange 

            // originalProduct
            var locationConfiguraionOriginalproduct = ProductLocationConfiguration.Create(
                locationType: LocationType.SLC,
                premiumUnitType: PremiumUnitType.Dollars,
                buyPremium: new Premium(2.0m),
                sellPremium: new Premium(2.0m),
                isAvailableForBuy: true,
                isAvailableForSell: true
                );

            var originalProduct = Product.Create(
                name: "Product",
                sku: new SKU("TEST-SKU"),
                weightInOz: new Weight(2.0m),
                metalType: MetalType.XAG,
                isAvailable: true
            );

            originalProduct.AddLocationConfiguration(locationConfiguraionOriginalproduct);


            //command
            var locationConfigCommand = new ProductModel.ProductLocationConfiguration
            {
                Location = LocationType.NY,
                PremiumUnitType = PremiumUnitType.Percentage,
                BuyPremium = 3.0m,
                SellPremium = 1.0m,
                IsAvailableForBuy = true,
                IsAvailableForSell = true
            };

            var command = new ProductUpdateCommand
            {
                Id = originalProduct.Id.Value,
                ProductName = "Product1",
                ProductSKU = "TEST - SKU1",
                WeightInOz = 3.0m,
                MetalType = MetalType.XAU,
                IsAvailableForTrading = true,
                LocationConfigurations = new List<ProductModel.ProductLocationConfiguration> { locationConfigCommand }
            };




            _productRepositoryMock
                .Setup(repo => repo.GetByIdOrThrowAsync(originalProduct.Id, false, It.IsAny<CancellationToken>(), x => x.LocationConfigurations))
                .ReturnsAsync(originalProduct);



            _productsServiceMock.Setup(service => service.IsSkuUnique(
            new SKU(command.ProductSKU),
            It.IsAny<CancellationToken>(),
            originalProduct.Id))
            .ReturnsAsync(true);

            //Assert
            await _handler.Handle(command, CancellationToken.None);

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(command.ProductName, originalProduct.Name);
            Assert.Equal(command.ProductSKU, originalProduct.SKU.Value);
            Assert.Equal(command.WeightInOz, originalProduct.WeightInOz.Value);
            Assert.Equal(command.MetalType, originalProduct.MetalType);
            Assert.Equal(command.IsAvailableForTrading, originalProduct.IsAvailable);

            foreach (var commandLocation in command.LocationConfigurations)
            {

                var matchingOriginalLocation = originalProduct.LocationConfigurations
                    .FirstOrDefault(ol => ol.LocationType == commandLocation.Location);

                Assert.NotNull(matchingOriginalLocation);

                Assert.Equal(commandLocation.PremiumUnitType, matchingOriginalLocation.PremiumUnitType);
                Assert.Equal(commandLocation.BuyPremium, matchingOriginalLocation.BuyPremium.Value);
                Assert.Equal(commandLocation.SellPremium, matchingOriginalLocation.SellPremium.Value);
                Assert.Equal(commandLocation.IsAvailableForBuy, matchingOriginalLocation.IsAvailableForBuy);
                Assert.Equal(commandLocation.IsAvailableForSell, matchingOriginalLocation.IsAvailableForSell);
            }
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenProductDoesNotExist()
        {
            //Assert

            var productId = new ProductId
                (
                    Guid.NewGuid()
                );

            var command = new ProductUpdateCommand
            {
                Id = productId.Value,
                ProductSKU = "NEW-SKU",
                ProductName = "Updated Product Name",
                MetalType = MetalType.XAU,
                WeightInOz = 10.5m,
                IsAvailableForTrading = true
            };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdOrThrowAsync(productId, false, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .ThrowsAsync(new NotFoundException($"Product with Guid {productId} not found."));

            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, It.IsAny<CancellationToken>()));

            _productRepositoryMock.Verify(x => x.GetByIdOrThrowAsync(productId, false, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);

            Assert.Equal($"Product with Guid {productId} not found.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenSkuIsNotUnique()
        {
            var originalProduct = Product.Create(
                name: "Product",
                sku: new SKU("TEST-SKU"),
                weightInOz: new Weight(2.0m),
                metalType: MetalType.XAG,
                isAvailable: true
            );

            var command = new ProductUpdateCommand
            {
                Id = originalProduct.Id.Value,
                ProductName = "Product1",
                ProductSKU = "TEST-SKU",
                WeightInOz = 3.0m,
                MetalType = MetalType.XAU,
                IsAvailableForTrading = true,
            };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdOrThrowAsync(originalProduct.Id, false, It.IsAny<CancellationToken>(), x => x.LocationConfigurations))
                .ReturnsAsync(originalProduct);

            _productsServiceMock.Setup(service => service.IsSkuUnique(
            new SKU(command.ProductSKU),
            It.IsAny<CancellationToken>(),
            originalProduct.Id))
            .ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<ProductSkuIsNotUniqueException>(() => _handler.Handle(command, It.IsAny<CancellationToken>()));

            Assert.Equal(exception.Message, $"A product with SKU '{command.ProductSKU}' already exists.");
        }
    }
}
