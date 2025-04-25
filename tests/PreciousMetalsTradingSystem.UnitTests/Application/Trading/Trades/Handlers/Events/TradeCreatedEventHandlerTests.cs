using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Trades.EventHandlers;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Notifications;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using LinqKit;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Trades.Handlers.Events
{
    public class TradeCreatedEventHandlerTests
    {
        private readonly Mock<ILogger<TradeCreatedEventHandler>> _loggerMock;
        private readonly Mock<IRealTimeNotificationPublisher> _publisherMock;
        private readonly Mock<IRepository<Trade, TradeId>> _tradeRepositoryMock;
        private readonly TradeCreatedEventHandler _handler;

        public TradeCreatedEventHandlerTests()
        {
            _loggerMock = new Mock<ILogger<TradeCreatedEventHandler>>();
            _publisherMock = new Mock<IRealTimeNotificationPublisher>();
            _tradeRepositoryMock = new Mock<IRepository<Trade, TradeId>>();

            _handler = new TradeCreatedEventHandler(
                _loggerMock.Object,
                _publisherMock.Object,
                _tradeRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldPublishNotification_WithCorrectData()
        {
            // Arrange
            var trade = CreateTestTrade(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.SLC);

            var domainEvent = TradeCreatedEvent.FromEntity(trade);
            var notification = new DomainEventNotification<TradeCreatedEvent>(domainEvent);

            SetupRepository(trade);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<TradeCreatedNotification>>(
                        n => n.Hub == HubType.Activity &&
                             n.Data.TradeId == trade.Id.Value.ToString() &&
                             n.Data.TradeNumber == trade.TradeNumber &&
                             n.Data.TradeType == TradeType.ClientTrade &&
                             n.Data.SideType == SideType.Buy &&
                             n.Data.LocationType == LocationType.SLC &&
                             n.Data.Items.Count() == 2),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            VerifyLogInformation($"Trade created: {trade.Id}, TradeNumber: {trade.TradeNumber}");
        }

        [Fact]
        public async Task Handle_ShouldMapTradeItems_Correctly()
        {
            // Arrange
            var trade = CreateTestTrade(
                TradeType.DealerTrade,
                SideType.Sell,
                LocationType.NY);

            var domainEvent = TradeCreatedEvent.FromEntity(trade);
            var notification = new DomainEventNotification<TradeCreatedEvent>(domainEvent);

            SetupRepository(trade);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<TradeCreatedNotification>>(n =>
                        VerifyTradeItemMapping(n.Data.Items.First(), trade.Items.First(), trade.Items.First().Product)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyNotes_ShouldUseEmptyString()
        {
            // Arrange
            var trade = CreateTestTrade(
                TradeType.DealerTrade,
                SideType.Buy,
                LocationType.IDS_DE);

            var domainEvent = TradeCreatedEvent.FromEntity(trade);
            var notification = new DomainEventNotification<TradeCreatedEvent>(domainEvent);

            SetupRepository(trade);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<TradeCreatedNotification>>(
                        n => n.Data.Note == string.Empty),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUseCorrectRepository_Parameters()
        {
            // Arrange
            var trade = CreateTestTrade(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.SLC);

            var domainEvent = TradeCreatedEvent.FromEntity(trade);
            var notification = new DomainEventNotification<TradeCreatedEvent>(domainEvent);

            // Setup repository to verify parameters
            _tradeRepositoryMock.Setup(r => r.StartQuery(true, true))
                .Returns(new List<Trade>().AsQueryable())
                .Verifiable();

            SetupRepository(trade);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _tradeRepositoryMock.Verify(r => r.StartQuery(true, true), Times.Once);
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

        private void SetupRepository(Trade trade)
        {
            // This simplification assumes the handler eventually needs a single trade
            // Rather than mocking the entire query chain, just make SingleAsync return our trade
            _tradeRepositoryMock
                .Setup(repo => repo.StartQuery(true, true))
                .Returns(new[] { trade }.AsQueryable().BuildMock());
        }

        private static bool VerifyTradeItemMapping(
            TradeItemData itemData,
            TradeItem tradeItem,
            Product product)
        {
            return itemData.TradeItemId == tradeItem.Id.Value.ToString() &&
                   itemData.ProductId == product.Id.Value.ToString() &&
                   itemData.ProductName == product.Name &&
                   itemData.ProductSKU == product.SKU &&
                   itemData.QuantityUnits == tradeItem.QuantityUnits.Value &&
                   itemData.SpotPricePerOz == tradeItem.SpotPricePerOz.Value &&
                   itemData.TradePricePerOz == tradeItem.TradePricePerOz.Value &&
                   itemData.PremiumPerOz == tradeItem.PremiumPerOz.Value &&
                   itemData.EffectivePricePerOz == tradeItem.EffectivePricePerOz.Value &&
                   itemData.TotalRevenue == tradeItem.TotalRevenue.Value &&
                   itemData.TotalEffectivePrice == tradeItem.TotalEffectivePrice.Value;
        }

        private static Trade CreateTestTrade(
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

            CreateTestTradeItems().ForEach(trade.AddItem);

            return trade;
        }

        private static IEnumerable<TradeItem> CreateTestTradeItems()
        {
            var product1 = CreateTestProduct("Gold Coin", "GC-001", 1.0m, MetalType.XAU);
            var product2 = CreateTestProduct("Silver Bar", "SB-001", 10.0m, MetalType.XAG);

            var item1 = CreateTestTradeItem(product1, 5, 1900.0m, 1900.0m, 50.0m, 1950.0m);
            var item2 = CreateTestTradeItem(product2, 10, 25.0m, 25.0m, 1.0m, 26.0m);

            return [item1, item2];
        }

        private static Product CreateTestProduct(
            string name, 
            string sku, 
            decimal weightInOz, 
            MetalType metalType)
        {
            return Product.Create(
                name,
                new SKU(sku),
                new Weight(weightInOz),
                metalType,
                true);
        }

        private static TradeItem CreateTestTradeItem(
            Product product,
            int quantityUnits,
            decimal spotPricePerOz,
            decimal tradePricePerOz,
            decimal premiumPerOz,
            decimal effectivePricePerOz)
        {
            // Create trade item using factory method
            var tradeItem = TradeItem.Create(
                SideType.Buy,
                product.Id,
                product.WeightInOz,
                new QuantityUnits(quantityUnits),
                new Money(spotPricePerOz),
                new Money(tradePricePerOz),
                new Premium(premiumPerOz),
                new Money(effectivePricePerOz))
                .SetProduct(product);

            return tradeItem;
        }

        #endregion
    }
}
