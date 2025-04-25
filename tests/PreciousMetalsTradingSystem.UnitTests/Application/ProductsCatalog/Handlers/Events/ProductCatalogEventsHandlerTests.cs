using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.EventHandlers;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models.Notifications;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.ProductsCatalog.Handlers.Events
{
    public class ProductCatalogEventsHandlerTests
    {
        private readonly Mock<ILogger<ProductCatalogEventsHandler>> _loggerMock;
        private readonly Mock<IRealTimeNotificationPublisher> _publisherMock;
        private readonly ProductCatalogEventsHandler _handler;

        public ProductCatalogEventsHandlerTests()
        {
            _loggerMock = new Mock<ILogger<ProductCatalogEventsHandler>>();
            _publisherMock = new Mock<IRealTimeNotificationPublisher>();
            _handler = new ProductCatalogEventsHandler(_loggerMock.Object, _publisherMock.Object);
        }

        [Fact]
        public async Task Handle_ProductCreatedEvent_ShouldPublishNotification()
        {
            // Arrange
            var productId = ProductId.New();
            var productName = "Test Product";
            var productSku = new SKU("TST-001");
            var weight = new Weight(10.5m);
            var metalType = MetalType.XAU;
            var locationConfigurations = new List<ProductCreatedEvent.LocationConfigurationData>
            {
                new(
                    LocationType.SLC,
                    PremiumUnitType.Percentage,
                    new Premium(2.5m),
                    new Premium(3.0m),
                    true,
                    true),
                new(
                    LocationType.NY,
                    PremiumUnitType.Dollars,
                    new Premium(10.0m),
                    new Premium(12.0m),
                    true,
                    false)
            };

            var domainEvent = new ProductCreatedEvent(
                productId,
                productName,
                productSku,
                weight,
                metalType,
                true,
                locationConfigurations);

            var notification = new DomainEventNotification<ProductCreatedEvent>(domainEvent);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<ProductCatalogChangedNotification>>(
                        n => n.Hub == HubType.Products &&
                             n.Data.ProductId == productId.Value.ToString() &&
                             n.Data.ChangeType == ChangeType.Created &&
                             n.Data.Name == productName &&
                             n.Data.SKU == productSku &&
                             n.Data.WeightInOz == weight.Value &&
                             n.Data.MetalType == metalType &&
                             n.Data.IsAvailable == true &&
                             VerifyLocationConfigurations(n.Data.LocationConfigurations, locationConfigurations)),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify logging
            VerifyLogInformation($"Product created: {productId}");
        }

        [Fact]
        public async Task Handle_ProductUpdatedEvent_ShouldPublishNotification()
        {
            // Arrange
            var productId = ProductId.New();
            var productName = "Updated Product";
            var productSku = new SKU("TST-002");
            var weight = new Weight(5.25m);
            var metalType = MetalType.XAG;
            var locationConfigurations = new List<ProductUpdatedEvent.LocationConfigurationData>
            {
                new(
                    LocationType.SLC,
                    PremiumUnitType.Dollars,
                    new Premium(5.0m),
                    new Premium(6.0m),
                    true,
                    true)
            };

            var domainEvent = new ProductUpdatedEvent(
                productId,
                productName,
                productSku,
                weight,
                metalType,
                false,
                locationConfigurations);

            var notification = new DomainEventNotification<ProductUpdatedEvent>(domainEvent);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<ProductCatalogChangedNotification>>(
                        n => n.Hub == HubType.Products &&
                             n.Data.ProductId == productId.Value.ToString() &&
                             n.Data.ChangeType == ChangeType.Updated &&
                             n.Data.Name == productName &&
                             n.Data.SKU == productSku &&
                             n.Data.WeightInOz == weight.Value &&
                             n.Data.MetalType == metalType &&
                             n.Data.IsAvailable == false &&
                             VerifyLocationConfigurations(n.Data.LocationConfigurations, locationConfigurations)),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify logging
            VerifyLogInformation($"Product updated: {productId}");
        }

        [Fact]
        public async Task Handle_ProductCreatedEvent_WithNoLocationConfigurations_ShouldPublishNotificationWithEmptyConfigurations()
        {
            // Arrange
            var productId = ProductId.New();
            var productName = "Test Product No Config";
            var productSku = new SKU("TST-003");
            var weight = new Weight(1.0m);
            var metalType = MetalType.XAU;
            var locationConfigurations = new List<ProductCreatedEvent.LocationConfigurationData>();

            var domainEvent = new ProductCreatedEvent(
                productId,
                productName,
                productSku,
                weight,
                metalType,
                true,
                locationConfigurations);

            var notification = new DomainEventNotification<ProductCreatedEvent>(domainEvent);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<ProductCatalogChangedNotification>>(
                        n => n.Hub == HubType.Products &&
                             n.Data.ProductId == productId.Value.ToString() &&
                             n.Data.ChangeType == ChangeType.Created &&
                             !n.Data.LocationConfigurations.Any()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ProductUpdatedEvent_WithMultipleLocationConfigurations_ShouldMapAllCorrectly()
        {
            // Arrange
            var productId = ProductId.New();
            var productName = "Updated Product Multiple Locations";
            var productSku = new SKU("TST-004");
            var weight = new Weight(2.0m);
            var metalType = MetalType.XAG;
            var locationConfigurations = new List<ProductUpdatedEvent.LocationConfigurationData>
            {
                new(
                    LocationType.SLC,
                    PremiumUnitType.Percentage,
                    new Premium(1.5m),
                    new Premium(2.0m),
                    true,
                    true),
                new(
                    LocationType.NY,
                    PremiumUnitType.Dollars,
                    new Premium(5.0m),
                    new Premium(7.0m),
                    true,
                    true),
                new(
                    LocationType.IDS_DE,
                    PremiumUnitType.Percentage,
                    new Premium(3.0m),
                    new Premium(4.0m),
                    false,
                    true)
            };

            var domainEvent = new ProductUpdatedEvent(
                productId,
                productName,
                productSku,
                weight,
                metalType,
                true,
                locationConfigurations);

            var notification = new DomainEventNotification<ProductUpdatedEvent>(domainEvent);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<ProductCatalogChangedNotification>>(
                        n => VerifyLocationConfigurationsCount(n.Data.LocationConfigurations, 3)),
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

        private bool VerifyLocationConfigurations(
            IEnumerable<LocationConfigurationData> actual,
            IEnumerable<ProductCreatedEvent.LocationConfigurationData> expected)
        {
            if (actual == null && expected == null)
                return true;

            if (actual == null || expected == null)
                return false;

            var actualList = actual.ToList();
            var expectedList = expected.ToList();

            if (actualList.Count != expectedList.Count)
                return false;

            for (int i = 0; i < actualList.Count; i++)
            {
                if (actualList[i].LocationType != expectedList[i].LocationType.ToString() ||
                    actualList[i].PremiumUnitType != expectedList[i].PremiumUnitType.ToString() ||
                    actualList[i].BuyPremium != expectedList[i].BuyPremium.Value ||
                    actualList[i].SellPremium != expectedList[i].SellPremium.Value ||
                    actualList[i].IsAvailableForBuy != expectedList[i].IsAvailableForBuy ||
                    actualList[i].IsAvailableForSell != expectedList[i].IsAvailableForSell)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool VerifyLocationConfigurations(
            IEnumerable<LocationConfigurationData> actual,
            IEnumerable<ProductUpdatedEvent.LocationConfigurationData> expected)
        {
            if (actual == null && expected == null)
                return true;

            if (actual == null || expected == null)
                return false;

            var actualList = actual.ToList();
            var expectedList = expected.ToList();

            if (actualList.Count != expectedList.Count)
                return false;

            for (int i = 0; i < actualList.Count; i++)
            {
                if (actualList[i].LocationType != expectedList[i].LocationType.ToString() ||
                    actualList[i].PremiumUnitType != expectedList[i].PremiumUnitType.ToString() ||
                    actualList[i].BuyPremium != expectedList[i].BuyPremium.Value ||
                    actualList[i].SellPremium != expectedList[i].SellPremium.Value ||
                    actualList[i].IsAvailableForBuy != expectedList[i].IsAvailableForBuy ||
                    actualList[i].IsAvailableForSell != expectedList[i].IsAvailableForSell)
                {
                    return false;
                }
            }

            return true;
        }

        private bool VerifyLocationConfigurationsCount(
            IEnumerable<LocationConfigurationData> configurations,
            int expectedCount)
        {
            return configurations.Count() == expectedCount;
        }

        #endregion
    }
}
