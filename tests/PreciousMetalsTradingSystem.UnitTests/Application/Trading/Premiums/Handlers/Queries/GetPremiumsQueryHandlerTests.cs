using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Premiums.Queries;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;
using System.Linq.Expressions;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Premiums.Handlers.Queries
{
    public class GetPremiumsQueryHandlerTests
    {
        private readonly Mock<IRepository<Product, ProductId>> _repositoryMock;
        private readonly GetPremiumsQueryHandler _handler;
        private readonly List<Product> _testProducts;

        public GetPremiumsQueryHandlerTests()
        {
            _repositoryMock = new Mock<IRepository<Product, ProductId>>();
            _handler = new GetPremiumsQueryHandler(_repositoryMock.Object);

            // Setup test products with their location configurations
            _testProducts = CreateTestProducts();

            // Setup repository to return test products
            _repositoryMock
                .Setup(r => r.GetAllAsync(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.Is<bool>(readOnly => readOnly == true),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.Is<string>(sort => sort == "SKU"),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Expression<Func<Product, object>>[]>()))
                .ReturnsAsync((items: _testProducts, totalCount: _testProducts.Count));
        }

        [Fact]
        public async Task Handle_WithNoFilters_ShouldReturnAllPremiums()
        {
            // Arrange
            var query = new GetPremiumsQuery
            {
                Location = null,
                ClientSide = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);

            // Should return premiums for all locations and all sides
            var expectedCount = _testProducts.Sum(p => p.LocationConfigurations.Count) * 2; // 2 sides (Buy/Sell)
            Assert.Equal(expectedCount, result.Items.Count);

            // Verify results are ordered correctly
            var items = result.Items.ToList();
            for (int i = 1; i < items.Count; i++)
            {
                var current = items[i];
                var previous = items[i - 1];

                // Check ordering: first by SKU, then by Location, then by Side
                if (previous.ProductSKU != current.ProductSKU)
                {
                    Assert.True(string.Compare(previous.ProductSKU, current.ProductSKU) <= 0);
                }
                else if (previous.Location != current.Location)
                {
                    Assert.True(previous.Location <= current.Location);
                }
                else
                {
                    Assert.True((int)previous.Side <= (int)current.Side);
                }
            }
        }

        [Theory]
        [InlineData(LocationType.SLC)]
        [InlineData(LocationType.NY)]
        public async Task Handle_WithLocationFilter_ShouldReturnPremiumsForSpecificLocation(LocationType location)
        {
            // Arrange
            var query = new GetPremiumsQuery
            {
                Location = location,
                ClientSide = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);

            // Check that all returned items have the correct location
            Assert.All(result.Items, item => Assert.Equal(location, item.Location));

            // Count should match products with this location * 2 sides
            var expectedProductCount = _testProducts.Count(p =>
                p.LocationConfigurations.Any(lc => lc.LocationType == location));
            var expectedPremiumCount = expectedProductCount * 2; // 2 sides (Buy/Sell)
            Assert.Equal(expectedPremiumCount, result.Items.Count);
        }

        [Theory]
        [InlineData(ClientSideType.Buy)]
        [InlineData(ClientSideType.Sell)]
        public async Task Handle_WithSideFilter_ShouldReturnPremiumsForSpecificSide(ClientSideType side)
        {
            // Arrange
            var query = new GetPremiumsQuery
            {
                Location = null,
                ClientSide = side
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);

            // Check that all returned items have the correct side
            Assert.All(result.Items, item => Assert.Equal(side, item.Side));

            // Count should match all products with all locations * 1 side
            var expectedCount = _testProducts.Sum(p => p.LocationConfigurations.Count);
            Assert.Equal(expectedCount, result.Items.Count);
        }

        [Theory]
        [InlineData(LocationType.SLC, ClientSideType.Buy)]
        [InlineData(LocationType.NY, ClientSideType.Sell)]
        public async Task Handle_WithBothFilters_ShouldReturnFilteredPremiums(LocationType location, ClientSideType side)
        {
            // Arrange
            var query = new GetPremiumsQuery
            {
                Location = location,
                ClientSide = side
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);

            // Check that all returned items have the correct location and side
            Assert.All(result.Items, item =>
            {
                Assert.Equal(location, item.Location);
                Assert.Equal(side, item.Side);
            });

            // Count should match products with this location * 1 side
            var expectedCount = _testProducts.Count(p =>
                p.LocationConfigurations.Any(lc => lc.LocationType == location));
            Assert.Equal(expectedCount, result.Items.Count);
        }

        [Fact]
        public async Task Handle_WithNoMatchingLocation_ShouldReturnEmptyResult()
        {
            // Arrange
            var query = new GetPremiumsQuery
            {
                Location = LocationType.IDS_DE, // No products with this location in test data
                ClientSide = null
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task Handle_ShouldCallRepositoryWithCorrectParameters()
        {
            // Arrange
            var query = new GetPremiumsQuery();
            var cancellationToken = new CancellationToken();

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            _repositoryMock.Verify(r => r.GetAllAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.Is<bool>(readOnly => readOnly == true),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.Is<string>(sort => sort == "SKU"),
                It.Is<CancellationToken>(ct => ct == cancellationToken),
                It.Is<Expression<Func<Product, object>>[]>(includes =>
                    includes.Length == 1 && includes[0].Body.ToString().Contains("LocationConfigurations"))),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldIncludeCorrectPremiumData()
        {
            // Arrange
            var query = new GetPremiumsQuery
            {
                Location = LocationType.SLC,
                ClientSide = ClientSideType.Buy
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var product1Premium = result.Items.FirstOrDefault(i => i.ProductSKU == "TST-001");
            Assert.NotNull(product1Premium);
            Assert.Equal("Test Product 1", product1Premium!.ProductName);
            Assert.Equal(LocationType.SLC, product1Premium.Location);
            Assert.Equal(ClientSideType.Buy, product1Premium.Side);
            Assert.Equal(PremiumUnitType.Percentage, product1Premium.PremiumUnitType);
            Assert.Equal(3m, product1Premium.PremiumPerOz);
        }

        #region Test Data Setup

        private static List<Product> CreateTestProducts()
        {
            var products = new List<Product>
            {
                CreateProduct("Test Product 1", "TST-001", 1.0m, MetalType.XAU, new List<(LocationType, PremiumUnitType, decimal, decimal, bool, bool)>
                {
                    (LocationType.SLC, PremiumUnitType.Percentage, 2.5m, 3.0m, true, true),
                    (LocationType.NY, PremiumUnitType.Dollars, 10.0m, 12.0m, true, true)
                }),

                CreateProduct("Test Product 2", "TST-002", 10.0m, MetalType.XAG, new List<(LocationType, PremiumUnitType, decimal, decimal, bool, bool)>
                {
                    (LocationType.SLC, PremiumUnitType.Dollars, 1.5m, 2.0m, true, true)
                }),

                CreateProduct("Test Product 3", "TST-003", 0.5m, MetalType.XAU, new List<(LocationType, PremiumUnitType, decimal, decimal, bool, bool)>
                {
                    (LocationType.NY, PremiumUnitType.Percentage, 5.0m, 6.0m, true, true)
                })
            };

            return products;
        }

        private static Product CreateProduct(
            string name,
            string sku,
            decimal weightInOz,
            MetalType metalType,
            List<(LocationType locationType, 
                PremiumUnitType premiumType, 
                decimal buyPremium, 
                decimal sellPremium, 
                bool isAvailableForBuy, 
                bool isAvailableForSell)> locations)
        {
            var product = Product.Create(
                name,
                new SKU(sku),
                new Weight(weightInOz),
                metalType,
                true);

            foreach (var (locationType, 
                premiumType, 
                buyPremium, 
                sellPremium, 
                isAvailableForBuy, 
                isAvailableForSell) in locations)
            {
                var locationConfig = ProductLocationConfiguration.Create(
                    locationType,
                    premiumType,
                    new Premium(buyPremium),
                    new Premium(sellPremium),
                    isAvailableForBuy,
                    isAvailableForSell);

                locationConfig.SetProduct(product.Id);
                product.AddLocationConfiguration(locationConfig);
            }

            return product;
        }

        #endregion
    }
}
