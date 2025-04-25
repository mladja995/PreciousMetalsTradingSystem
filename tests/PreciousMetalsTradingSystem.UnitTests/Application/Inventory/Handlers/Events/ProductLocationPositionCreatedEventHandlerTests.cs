using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Inventory.EventHandlers;
using PreciousMetalsTradingSystem.Application.Inventory.Models.Notifications;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Inventory.Handlers.Events
{
    public class ProductLocationPositionCreatedEventHandlerTests
    {
        private readonly Mock<ILogger<ProductLocationPositionCreatedEventHandler>> _loggerMock;
        private readonly Mock<IRealTimeNotificationPublisher> _publisherMock;
        private readonly Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>> _positionRepositoryMock;
        private readonly ProductLocationPositionCreatedEventHandler _handler;

        public ProductLocationPositionCreatedEventHandlerTests()
        {
            _loggerMock = new Mock<ILogger<ProductLocationPositionCreatedEventHandler>>();
            _publisherMock = new Mock<IRealTimeNotificationPublisher>();
            _positionRepositoryMock = new Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>>();

            _handler = new ProductLocationPositionCreatedEventHandler(
                _loggerMock.Object,
                _publisherMock.Object,
                _positionRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldPublishNotification_WithCorrectData()
        {
            // Arrange
            var position = CreateTestPosition();
            var domainEvent = ProductLocationPositionCreatedEvent.FromEntity(position);
            var notification = new DomainEventNotification<ProductLocationPositionCreatedEvent>(domainEvent);

            SetupRepository(position);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<InventoryChangedNotification>>(
                        n => n.Hub == HubType.Inventory &&
                             n.Data.PositionId == position.Id.Value.ToString() &&
                             n.Data.ProductId == position.ProductId.Value.ToString() &&
                             n.Data.ProductName == position.Product.Name &&
                             n.Data.SKU == position.Product.SKU &&
                             n.Data.LocationType == position.LocationType &&
                             n.Data.SideType == position.SideType &&
                             n.Data.PositionType == position.Type &&
                             n.Data.QuantityUnits == position.QuantityUnits &&
                             n.Data.PositionUnits == position.PositionUnits),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            VerifyLogInformation($"Inventory position created: {position.Id} for product: {position.ProductId}");
        }

        [Fact]
        public async Task Handle_ShouldIncludeTradeInformation_InNotification()
        {
            // Arrange
            var position = CreateTestPosition();
            var domainEvent = ProductLocationPositionCreatedEvent.FromEntity(position);
            var notification = new DomainEventNotification<ProductLocationPositionCreatedEvent>(domainEvent);

            SetupRepository(position);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<InventoryChangedNotification>>(
                        n => n.Data.Reference == position.Trade.TradeNumber &&
                             n.Data.TradeType == position.Trade.Type),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUseCorrectRepository_Parameters()
        {
            // Arrange
            var position = CreateTestPosition();
            var domainEvent = ProductLocationPositionCreatedEvent.FromEntity(position);
            var notification = new DomainEventNotification<ProductLocationPositionCreatedEvent>(domainEvent);

            // Setup repository to verify parameters
            _positionRepositoryMock.Setup(r => r.StartQuery(true, true))
                .Returns(new List<ProductLocationPosition>().AsQueryable())
                .Verifiable();

            SetupRepository(position);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _positionRepositoryMock.Verify(r => r.StartQuery(true, true), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUseSplitQuery_ForRelatedEntities()
        {
            // Arrange
            var position = CreateTestPosition();
            var domainEvent = ProductLocationPositionCreatedEvent.FromEntity(position);
            var notification = new DomainEventNotification<ProductLocationPositionCreatedEvent>(domainEvent);

            var mockQueryable = new List<ProductLocationPosition> { position }.AsQueryable().BuildMock();

            // Verify that we're using split query and including the right navigation properties
            _positionRepositoryMock.Setup(r => r.StartQuery(true, true))
                .Returns(mockQueryable)
                .Verifiable();

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _positionRepositoryMock.Verify(r => r.StartQuery(true, true), Times.Once);
        }

        [Theory]
        [InlineData(LocationType.NY, PositionSideType.Out)]
        [InlineData(LocationType.IDS_DE, PositionSideType.In)]
        public async Task Handle_WithDifferentLocationsAndSides_ShouldPublishCorrectNotification(
            LocationType locationType,
            PositionSideType sideType)
        {
            // Arrange
            var position = CreateTestPosition(locationType, sideType);
            var domainEvent = ProductLocationPositionCreatedEvent.FromEntity(position);
            var notification = new DomainEventNotification<ProductLocationPositionCreatedEvent>(domainEvent);

            SetupRepository(position);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<InventoryChangedNotification>>(
                        n => n.Data.LocationType == locationType &&
                             n.Data.SideType == sideType),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldPreserveTimestamp_InNotification()
        {
            // Arrange
            var specificTimestamp = new DateTime(2025, 3, 15, 10, 30, 0, DateTimeKind.Utc);
            var position = CreateTestPosition();

            // Set a specific timestamp to verify it's properly passed through
            typeof(ProductLocationPosition).GetProperty("TimestampUtc")!.SetValue(position, specificTimestamp);

            var domainEvent = ProductLocationPositionCreatedEvent.FromEntity(position);
            var notification = new DomainEventNotification<ProductLocationPositionCreatedEvent>(domainEvent);

            SetupRepository(position);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<InventoryChangedNotification>>(
                        n => n.Data.TimestampUtc == specificTimestamp),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #region Helper Methods

        private void VerifyLogInformation(string expectedMessage)
        {
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains(expectedMessage)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void SetupRepository(ProductLocationPosition position)
        {
            var mockQueryable = new List<ProductLocationPosition> { position }.AsQueryable().BuildMock();

            _positionRepositoryMock
                .Setup(repo => repo.StartQuery(true, true))
                .Returns(mockQueryable);
        }

        private static ProductLocationPosition CreateTestPosition(
            LocationType locationType = LocationType.SLC,
            PositionSideType sideType = PositionSideType.In)
        {
            var productId = ProductId.New();
            var product = CreateTestProduct(productId, "Gold Coin", "GC-001", 1.0m, MetalType.XAU);

            var tradeId = TradeId.New();
            // Convert PositionSideType to SideType for trade creation
            var tradeSideType = sideType == PositionSideType.In ? SideType.Buy : SideType.Sell;
            var trade = CreateTestTrade(tradeId, TradeType.ClientTrade, tradeSideType, locationType);

            var position = ProductLocationPosition.Create(
                productId,
                tradeId,
                locationType,
                sideType,
                PositionType.AvailableForTrading,
                new QuantityUnits(10),
                new PositionQuantityUnits(100));

            // Set navigation properties
            typeof(ProductLocationPosition).GetProperty("Product")!.SetValue(position, product);
            typeof(ProductLocationPosition).GetProperty("Trade")!.SetValue(position, trade);

            return position;
        }

        private static PreciousMetalsTradingSystem.Domain.Entities.Product CreateTestProduct(
            ProductId id,
            string name,
            string sku,
            decimal weightInOz,
            MetalType metalType)
        {
            var product = PreciousMetalsTradingSystem.Domain.Entities.Product.Create(
                name,
                new SKU(sku),
                new Weight(weightInOz),
                metalType,
                true);

            // Override the generated ID with our fixed one
            typeof(PreciousMetalsTradingSystem.Domain.Entities.Product).GetProperty("Id")!.SetValue(product, id);

            return product;
        }

        private static Trade CreateTestTrade(
            TradeId id,
            TradeType tradeType,
            SideType sideType,
            LocationType locationType,
            string? note = null)
        {
            var trade = Trade.Create(
                tradeType,
                sideType,
                locationType,
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                note);

            // Override the generated ID with our fixed one
            typeof(Trade).GetProperty("Id")!.SetValue(trade, id);

            return trade;
        }

        #endregion
    }
}
