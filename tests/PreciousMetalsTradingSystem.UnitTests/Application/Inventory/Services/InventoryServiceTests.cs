using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Domain.DomainServices;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MockQueryable;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Inventory.Services
{
    public class InventoryServiceTests
    {
        private readonly Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>> _repositoryMock;
        private readonly Mock<IInventoryPositionValidator> _validatorMock;
        private readonly InventoryService _service;

        public InventoryServiceTests()
        {
            _repositoryMock = new Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>>();
            _validatorMock = new Mock<IInventoryPositionValidator>();

            _service = new InventoryService(_repositoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task GetRunningPositionBalanceAsync_WhenCacheIsEmpty_FetchesFromDatabase()
        {
            // Arrange
            var productId = ProductId.New();
            var location = LocationType.SLC;
            var positionType = PositionType.AvailableForTrading;
            var positionUnits = new PositionQuantityUnits(1000);

            var position = CreateTestPosition(productId, location, positionType, positionUnits);
            SetupRepository(position);

            // Act
            var result = await _service.GetRunningPositionBalanceAsync(productId, location, positionType);

            // Assert
            Assert.Equal(positionUnits, result);
            _repositoryMock.Verify(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GetRunningPositionBalanceAsync_WhenCacheHasEntry_ReturnsFromCache()
        {
            // Arrange
            var productId = ProductId.New();
            var location = LocationType.SLC;
            var positionType = PositionType.AvailableForTrading;
            var positionUnits = new PositionQuantityUnits(1000);

            var position = CreateTestPosition(productId, location, positionType, positionUnits);
            SetupRepository(position);

            // First call to populate cache
            await _service.GetRunningPositionBalanceAsync(productId, location, positionType);

            // Reset mock to verify it's not called again
            _repositoryMock.Reset();
            SetupRepository(position);

            // Act
            var result = await _service.GetRunningPositionBalanceAsync(productId, location, positionType);

            // Assert
            Assert.Equal(positionUnits, result);
            _repositoryMock.Verify(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task GetRunningPositionBalanceAsync_WhenNoPositionsExist_ReturnsZeroBalance()
        {
            // Arrange
            var productId = ProductId.New();
            var location = LocationType.SLC;
            var positionType = PositionType.AvailableForTrading;
            SetupEmptyRepository();

            // Act
            var result = await _service.GetRunningPositionBalanceAsync(productId, location, positionType);

            // Assert
            Assert.Equal(new PositionQuantityUnits(0), result);
        }

        [Fact]
        public async Task CreatePositionAsync_UpdatesPositionBalanceCache()
        {
            // Arrange
            var productId = ProductId.New();
            var tradeId = TradeId.New();
            var location = LocationType.SLC;
            var positionType = PositionType.AvailableForTrading;
            var initialPositionUnits = new PositionQuantityUnits(1000);
            var quantityUnits = new QuantityUnits(200);

            var lastPosition = CreateTestPosition(productId, location, positionType, initialPositionUnits);
            SetupRepository(lastPosition);

            // Act
            var result = await _service.CreatePositionAsync(
                productId,
                tradeId,
                location,
                positionType,
                PositionSideType.In,
                quantityUnits);

            // Assert
            // Get balance from cache to verify it was updated
            var updatedBalance = await _service.GetRunningPositionBalanceAsync(productId, location, positionType);
            Assert.Equal(result.PositionUnits, updatedBalance);
            Assert.NotEqual(initialPositionUnits, updatedBalance);
        }

        [Fact]
        public async Task HasEnoughQuantityForSellAsync_CallsValidator()
        {
            // Arrange
            var productId = ProductId.New();
            var location = LocationType.SLC;
            var positionType = PositionType.AvailableForTrading;
            var currentPosition = new PositionQuantityUnits(1000);
            var requestedQuantity = new QuantityUnits(500);

            var position = CreateTestPosition(productId, location, positionType, currentPosition);
            SetupRepository(position);

            var quantityPerProduct = new Dictionary<ProductId, QuantityUnits> { { productId, requestedQuantity } };

            _validatorMock.Setup(v => v.IsSufficientForSell(requestedQuantity, currentPosition))
                .Returns(true);

            // Act
            var result = await _service.HasEnoughQuantityForSellAsync(location, positionType, quantityPerProduct);

            // Assert
            Assert.True(result);
            _validatorMock.Verify(v => v.IsSufficientForSell(requestedQuantity, currentPosition), Times.Once);
        }

        [Fact]
        public async Task CreatePositionAsync_MultiplePositions_UsesCorrectBalances()
        {
            // Arrange
            var productId = ProductId.New();
            var tradeId = TradeId.New();
            var location = LocationType.SLC;
            var positionType = PositionType.AvailableForTrading;
            var initialPositionUnits = new PositionQuantityUnits(1000);
            var quantity1 = new QuantityUnits(200);
            var quantity2 = new QuantityUnits(300);
            var quantity3 = new QuantityUnits(150);

            var lastPosition = CreateTestPosition(productId, location, positionType, initialPositionUnits);
            SetupRepository(lastPosition);

            // Act - Create multiple positions
            var position1 = await _service.CreatePositionAsync(
                productId,
                tradeId,
                location,
                positionType,
                PositionSideType.Out,
                quantity1);

            var position2 = await _service.CreatePositionAsync(
                productId,
                tradeId,
                location,
                positionType,
                PositionSideType.Out,
                quantity2);

            var position3 = await _service.CreatePositionAsync(
                productId,
                tradeId,
                location,
                positionType,
                PositionSideType.Out,
                quantity3);

            // Assert
            // Calculate expected balances
            var expectedBalance1 = new PositionQuantityUnits(initialPositionUnits - quantity1.Value);
            var expectedBalance2 = new PositionQuantityUnits(expectedBalance1.Value - quantity2.Value);
            var expectedBalance3 = new PositionQuantityUnits(expectedBalance2.Value - quantity3.Value);

            // First position should use initial balance
            Assert.Equal(expectedBalance1, position1.PositionUnits);

            // Second position should use balance after first position
            Assert.Equal(expectedBalance2, position2.PositionUnits);

            // Third position should use balance after second position
            Assert.Equal(expectedBalance3, position3.PositionUnits);

            // Verify repository was called only once to get the initial balance
            _repositoryMock.Verify(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task CreatePositionAsync_MultipleLocationsAndTypes_TracksEachCombinationCorrectly()
        {
            // Arrange
            var productId = ProductId.New();
            var tradeId = TradeId.New();
            var slcLocation = LocationType.SLC;
            var nyLocation = LocationType.NY;
            var availableType = PositionType.AvailableForTrading;

            var initialSlcAvailable = new PositionQuantityUnits(1000);
            var initialNyAvailable = new PositionQuantityUnits(500);

            var slcQuantity = new QuantityUnits(200);
            var nyQuantity = new QuantityUnits(100);

            var slcPosition = CreateTestPosition(productId, slcLocation, availableType, initialSlcAvailable);
            var nyPosition = CreateTestPosition(productId, nyLocation, availableType, initialNyAvailable);

            SetupRepositoryWithMultiplePositions(slcPosition, nyPosition);

            // Act
            var slcPos1 = await _service.CreatePositionAsync(
                productId,
                tradeId,
                slcLocation,
                availableType,
                PositionSideType.Out,
                slcQuantity);

            var nyPos1 = await _service.CreatePositionAsync(
                productId,
                tradeId,
                nyLocation,
                availableType,
                PositionSideType.Out,
                nyQuantity);

            var slcPos2 = await _service.CreatePositionAsync(
                productId,
                tradeId,
                slcLocation,
                availableType,
                PositionSideType.Out,
                slcQuantity);

            // Assert
            // Calculate expected balances
            var expectedSlcBalance1 = new PositionQuantityUnits(initialSlcAvailable.Value - slcQuantity.Value);
            var expectedNyBalance1 = new PositionQuantityUnits(initialNyAvailable.Value - nyQuantity.Value);
            var expectedSlcBalance2 = new PositionQuantityUnits(expectedSlcBalance1.Value - slcQuantity.Value);

            // SLC balance should be tracked separately from NY balance
            Assert.Equal(expectedSlcBalance1, slcPos1.PositionUnits);
            Assert.Equal(expectedNyBalance1, nyPos1.PositionUnits);
            Assert.Equal(expectedSlcBalance2, slcPos2.PositionUnits);

            // Final balances should be correct
            var finalSlcBalance = await _service.GetRunningPositionBalanceAsync(productId, slcLocation, availableType);
            var finalNyBalance = await _service.GetRunningPositionBalanceAsync(productId, nyLocation, availableType);

            Assert.Equal(expectedSlcBalance2, finalSlcBalance);
            Assert.Equal(expectedNyBalance1, finalNyBalance);
        }

        [Theory]
        [InlineData(PositionSideType.In, 1000, 200, 1200)]
        [InlineData(PositionSideType.Out, 1000, 200, 800)]
        [InlineData(PositionSideType.In, 0, 500, 500)]
        [InlineData(PositionSideType.Out, 500, 500, 0)]
        public async Task CreatePositionAsync_WithDifferentSideTypes_CalculatesBalanceCorrectly(
            PositionSideType sideType, int initialBalance, int quantity, int expectedBalance)
        {
            // Arrange
            var productId = ProductId.New();
            var tradeId = TradeId.New();
            var location = LocationType.SLC;
            var positionType = PositionType.AvailableForTrading;
            var initialPositionUnits = new PositionQuantityUnits(initialBalance);
            var quantityUnits = new QuantityUnits(quantity);

            var lastPosition = CreateTestPosition(productId, location, positionType, initialPositionUnits);
            SetupRepository(lastPosition);

            // Act
            var position = await _service.CreatePositionAsync(
                productId,
                tradeId,
                location,
                positionType,
                sideType,
                quantityUnits);

            // Assert
            Assert.Equal(new PositionQuantityUnits(expectedBalance), position.PositionUnits);

            // Verify cache was updated
            var cachedBalance = await _service.GetRunningPositionBalanceAsync(productId, location, positionType);
            Assert.Equal(new PositionQuantityUnits(expectedBalance), cachedBalance);
        }

        [Fact]
        public async Task HasEnoughQuantityForSellAsync_WithMultipleProducts_ChecksAllProducts()
        {
            // Arrange
            var product1Id = ProductId.New();
            var product2Id = ProductId.New();
            var location = LocationType.SLC;
            var positionType = PositionType.AvailableForTrading;

            var position1Units = new PositionQuantityUnits(1000);
            var position2Units = new PositionQuantityUnits(500);

            var requested1Units = new QuantityUnits(800);
            var requested2Units = new QuantityUnits(400);

            var position1 = CreateTestPosition(product1Id, location, positionType, position1Units);
            var position2 = CreateTestPosition(product2Id, location, positionType, position2Units);

            SetupRepositoryWithMultiplePositions(position1, position2);

            var quantityPerProduct = new Dictionary<ProductId, QuantityUnits>
            {
                { product1Id, requested1Units },
                { product2Id, requested2Units }
            };

            _validatorMock.Setup(v => v.IsSufficientForSell(requested1Units, position1Units)).Returns(true);
            _validatorMock.Setup(v => v.IsSufficientForSell(requested2Units, position2Units)).Returns(true);

            // Act
            var result = await _service.HasEnoughQuantityForSellAsync(location, positionType, quantityPerProduct);

            // Assert
            Assert.True(result);
            _validatorMock.Verify(v => v.IsSufficientForSell(requested1Units, position1Units), Times.Once);
            _validatorMock.Verify(v => v.IsSufficientForSell(requested2Units, position2Units), Times.Once);
        }

        [Fact]
        public async Task HasEnoughQuantityForSellAsync_WhenOneProductHasInsufficientQuantity_ReturnsFalse()
        {
            // Arrange
            var product1Id = ProductId.New();
            var product2Id = ProductId.New();
            var location = LocationType.SLC;
            var positionType = PositionType.AvailableForTrading;

            var position1Units = new PositionQuantityUnits(1000);
            var position2Units = new PositionQuantityUnits(300);

            var requested1Units = new QuantityUnits(800);
            var requested2Units = new QuantityUnits(400); // More than available

            var position1 = CreateTestPosition(product1Id, location, positionType, position1Units);
            var position2 = CreateTestPosition(product2Id, location, positionType, position2Units);

            SetupRepositoryWithMultiplePositions(position1, position2);

            var quantityPerProduct = new Dictionary<ProductId, QuantityUnits>
            {
                { product1Id, requested1Units },
                { product2Id, requested2Units }
            };

            _validatorMock.Setup(v => v.IsSufficientForSell(requested1Units, position1Units)).Returns(true);
            _validatorMock.Setup(v => v.IsSufficientForSell(requested2Units, position2Units)).Returns(false);

            // Act
            var result = await _service.HasEnoughQuantityForSellAsync(location, positionType, quantityPerProduct);

            // Assert
            Assert.False(result);
            // Should short-circuit on the first failure
            _validatorMock.Verify(v => v.IsSufficientForSell(It.IsAny<QuantityUnits>(), It.IsAny<PositionQuantityUnits>()), Times.AtLeastOnce());
        }

        private ProductLocationPosition CreateTestPosition(
            ProductId productId,
            LocationType location,
            PositionType positionType,
            PositionQuantityUnits positionUnits)
        {
            // Create position using reflection to set private properties directly for testing
            var position = new ProductLocationPosition();

            typeof(ProductLocationPosition).GetProperty("Id")!.SetValue(position, ProductLocationPositionId.New());
            typeof(ProductLocationPosition).GetProperty("ProductId")!.SetValue(position, productId);
            typeof(ProductLocationPosition).GetProperty("TradeId")!.SetValue(position, TradeId.New());
            typeof(ProductLocationPosition).GetProperty("LocationType")!.SetValue(position, location);
            typeof(ProductLocationPosition).GetProperty("SideType")!.SetValue(position, PositionSideType.In);
            typeof(ProductLocationPosition).GetProperty("Type")!.SetValue(position, positionType);
            typeof(ProductLocationPosition).GetProperty("TimestampUtc")!.SetValue(position, DateTime.UtcNow);
            typeof(ProductLocationPosition).GetProperty("QuantityUnits")!.SetValue(position, new QuantityUnits(100));
            typeof(ProductLocationPosition).GetProperty("PositionUnits")!.SetValue(position, positionUnits);

            return position;
        }

        private void SetupRepository(ProductLocationPosition position)
        {
            var queryable = new List<ProductLocationPosition> { position }.AsQueryable().BuildMock();

            _repositoryMock.Setup(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(queryable);
        }

        private void SetupEmptyRepository()
        {
            var queryable = new List<ProductLocationPosition>().AsQueryable().BuildMock();

            _repositoryMock.Setup(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(queryable);
        }

        private void SetupRepositoryWithMultiplePositions(params ProductLocationPosition[] positions)
        {
            var queryable = positions.ToList().AsQueryable().BuildMock();

            _repositoryMock.Setup(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(queryable);
        }
    }
}
