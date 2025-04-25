using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Commands.Create;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Exceptions;
using ProductModel = PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.ProductsCatalog.Handlers.Commands
{
    public class ProductCreateCommandHandlerTests
    {

        private readonly Mock<IRepository<Product, ProductId>> _productRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProductsService> _productsServiceMock;
        private readonly ProductCreateCommandHandler _handler;

        public ProductCreateCommandHandlerTests()
        {
            _productRepositoryMock = new Mock<IRepository<Product, ProductId>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _productsServiceMock = new Mock<IProductsService>();
            _handler = new ProductCreateCommandHandler(_unitOfWorkMock.Object, _productRepositoryMock.Object, _productsServiceMock.Object);
        }


        [Fact]

        public async Task Handle_ShouldSucceed_WhenProductIsCreatedSuccessfuly()
        {
            //Arange
            var locationConfigurationCommand = new ProductModel.ProductLocationConfiguration
            {
                Location = LocationType.SLC,
                PremiumUnitType = PremiumUnitType.Percentage,
                BuyPremium = 3.0m,
                SellPremium = 3.0m,
                IsAvailableForBuy = true,
                IsAvailableForSell = true,
            };
            var command = new ProductCreateCommand
            {
                ProductSKU = "ABCD",
                ProductName = "Test-Product",
                MetalType = MetalType.XAU,
                WeightInOz = 2.0m,
                IsAvailableForTrading = true,
                LocationConfigurations = new List<ProductModel.ProductLocationConfiguration> { locationConfigurationCommand }
            };

            //Simulate validation of SKU uniqueness
            _productsServiceMock.Setup(service => service.IsSkuUnique(
             new SKU(command.ProductSKU),
             It.IsAny<CancellationToken>(), null)).ReturnsAsync(true);

            //Act
            var result = await _handler.Handle(command, CancellationToken.None);

            //Assert
            _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsType<Guid>(result);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenSkuIsNotUnique()
        {
            var command = new ProductCreateCommand
            {
                ProductSKU = "ABCD",
                ProductName = "Test-Product",
                MetalType = MetalType.XAU,
                WeightInOz = 2.0m,
                IsAvailableForTrading = true,
            };

            _productsServiceMock.Setup(service => service.IsSkuUnique(
            new SKU(command.ProductSKU),
            It.IsAny<CancellationToken>(), null)).ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<ProductSkuIsNotUniqueException>(() => _handler.Handle(command, It.IsAny<CancellationToken>()));

            Assert.Equal(exception.Message, $"A product with SKU '{command.ProductSKU}' already exists.");
        }
    }
}
