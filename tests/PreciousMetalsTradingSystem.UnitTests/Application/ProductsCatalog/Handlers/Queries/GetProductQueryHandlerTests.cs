using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetSingle;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


namespace PreciousMetalsTradingSystem.UnitTests.Application.ProductsCatalog.Handlers.Queries
{
    public class GetProductQueryHandlerTests
    {
        private readonly Mock<IRepository<Product, ProductId>> _productRepositoryMock;
        private readonly GetProductQueryHandler _handler;

        public GetProductQueryHandlerTests()
        {
            _productRepositoryMock = new Mock<IRepository<Product, ProductId>>();
            _handler = new GetProductQueryHandler(_productRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnProduct_WhenProductExists()
        {
            //Arrange
            var productGuid = Guid.NewGuid();
            var productId = new ProductId
            (
                productGuid
            );

            var command = new GetProductQuery
            {
                Id = productId.Value
            };
            var expectedProduct = Product.Create(
                name: "Test Product",
                sku: new SKU("TEST-SKU"),
                weightInOz: new Weight(5.0m),
                metalType: MetalType.XAU,
                isAvailable: true
            );


            var locationConfiguraion = ProductLocationConfiguration.Create(
                locationType: LocationType.SLC,
                premiumUnitType: PremiumUnitType.Dollars,
                buyPremium: new Premium(2.0m),
                sellPremium: new Premium(2.0m),
                isAvailableForBuy: true,
                isAvailableForSell: true
                );

            expectedProduct.AddLocationConfiguration(locationConfiguraion);

            _productRepositoryMock
                .Setup(repo => repo.GetByIdOrThrowAsync(productId, false, It.IsAny<CancellationToken>(), x => x.LocationConfigurations))
                .ReturnsAsync(expectedProduct);

            //Act
            var result = await _handler.Handle(command, It.IsAny<CancellationToken>());

            _productRepositoryMock.Verify(x => x.GetByIdOrThrowAsync(It.IsAny<ProductId>(), false, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProduct.Id.Value, result.Id);
            Assert.Equal(expectedProduct.Name, result.Name);
            Assert.Equal(expectedProduct.SKU.Value, result.SKU);
            Assert.Equal(expectedProduct.WeightInOz.Value, result.WeightInOz);
            Assert.Equal(expectedProduct.MetalType, result.MetalType);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenProduct_DoesNotExist()
        {
            var productId = new ProductId
            (
                Guid.NewGuid()
            );

            var command = new GetProductQuery
            {
                Id = productId.Value
            };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdOrThrowAsync(productId, false, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Product, object>>[]>()))
                .ThrowsAsync(new NotFoundException($"Product with Guid {productId.Value} not found."));


            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, It.IsAny<CancellationToken>()));

            _productRepositoryMock.Verify(x => x.GetByIdOrThrowAsync(productId, false, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<Product, object>>[]>()), Times.Once);

            Assert.Equal($"Product with Guid {productId.Value} not found.", exception.Message);
        }
    }
}
