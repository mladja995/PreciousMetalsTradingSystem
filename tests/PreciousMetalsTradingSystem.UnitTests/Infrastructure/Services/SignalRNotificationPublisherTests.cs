using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Infrastructure.SignalR.Hubs;
using PreciousMetalsTradingSystem.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Infrastructure.Services
{
    public class SignalRNotificationPublisherTests
    {
        private readonly Mock<IHubContext<ActivityHub>> _activityHubContextMock;
        private readonly Mock<IHubContext<ProductsHub>> _productsHubContextMock;
        private readonly Mock<IHubContext<FinancialsHub>> _financialsHubContextMock;
        private readonly Mock<IHubContext<InventoryHub>> _inventoryHubContextMock;
        private readonly Mock<IHubContext<HedgingHub>> _hedgingHubContextMock;
        private readonly Mock<ILogger<SignalRNotificationPublisher>> _loggerMock;
        private readonly Mock<IHubClients> _hubClientsMock;
        private readonly Mock<IClientProxy> _clientProxyMock;

        private readonly SignalRNotificationPublisher _publisher;

        public SignalRNotificationPublisherTests()
        {
            _activityHubContextMock = new Mock<IHubContext<ActivityHub>>();
            _productsHubContextMock = new Mock<IHubContext<ProductsHub>>();
            _financialsHubContextMock = new Mock<IHubContext<FinancialsHub>>();
            _inventoryHubContextMock = new Mock<IHubContext<InventoryHub>>();
            _hedgingHubContextMock = new Mock<IHubContext<HedgingHub>>();
            _loggerMock = new Mock<ILogger<SignalRNotificationPublisher>>();

            // Setup hub clients
            _hubClientsMock = new Mock<IHubClients>();
            _clientProxyMock = new Mock<IClientProxy>();
            _hubClientsMock.Setup(clients => clients.All).Returns(_clientProxyMock.Object);

            // Setup each hub context to return the hub clients
            _activityHubContextMock.Setup(context => context.Clients).Returns(_hubClientsMock.Object);
            _productsHubContextMock.Setup(context => context.Clients).Returns(_hubClientsMock.Object);
            _financialsHubContextMock.Setup(context => context.Clients).Returns(_hubClientsMock.Object);
            _inventoryHubContextMock.Setup(context => context.Clients).Returns(_hubClientsMock.Object);
            _hedgingHubContextMock.Setup(context => context.Clients).Returns(_hubClientsMock.Object);

            _publisher = new SignalRNotificationPublisher(
                _activityHubContextMock.Object,
                _productsHubContextMock.Object,
                _financialsHubContextMock.Object,
                _inventoryHubContextMock.Object,
                _hedgingHubContextMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task PublishAsync_ShouldSendNotification_WhenHubExists()
        {
            // Arrange
            var notification = new RealTimeNotification<string>(HubType.Activity, "Test Data");

            _clientProxyMock
                .Setup(c => c.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _publisher.PublishAsync(notification);

            // Assert
            _clientProxyMock.Verify(
                c => c.SendCoreAsync(
                    "String", // Method name is the type name
                    It.Is<object[]>(o => o.Length == 1 && o[0].Equals("Test Data")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishAsync_ShouldLogWarning_WhenHubDoesNotExist()
        {
            // Arrange
            // Using an enum value that doesn't exist in the dictionary
            var invalidHubType = (HubType)999;
            var notification = new RealTimeNotification<string>(invalidHubType, "Test Data");

            // Act
            await _publisher.PublishAsync(notification);

            // Assert
            _clientProxyMock.Verify(
                c => c.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            // Verify warning was logged
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains($"Hub '{invalidHubType}' not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishAsync_ShouldHandleException_WhenSendingFails()
        {
            // Arrange
            var notification = new RealTimeNotification<string>(HubType.Products, "Test Data");

            _clientProxyMock
                .Setup(c => c.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert - should not throw
            await _publisher.PublishAsync(notification);

            // Verify error was logged
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error publishing notification")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData(HubType.Activity)]
        [InlineData(HubType.Products)]
        [InlineData(HubType.Financials)]
        [InlineData(HubType.Inventory)]
        [InlineData(HubType.Hedging)]
        public async Task PublishAsync_ShouldUseCorrectHub_ForEachHubType(HubType hubType)
        {
            // Arrange
            var notification = new RealTimeNotification<string>(hubType, "Test Data");

            // Act
            await _publisher.PublishAsync(notification);

            // Assert
            _clientProxyMock.Verify(
                c => c.SendCoreAsync(
                    "String",
                    It.Is<object[]>(o => o.Length == 1 && o[0].Equals("Test Data")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishAsync_ShouldHandleComplexDataTypes()
        {
            // Arrange
            var complexData = new TestNotification(123, "Test Name", true);

            var notification = new RealTimeNotification<TestNotification>(HubType.Activity, complexData);

            // Act
            await _publisher.PublishAsync(notification);

            // Assert
            _clientProxyMock.Verify(
                c => c.SendCoreAsync(
                    "TestNotification",
                    It.Is<object[]>(o => o.Length == 1 && o[0] is TestNotification),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // Test data class for complex notification data
        private record TestNotification(int Id, string Name, bool Active) { }
    }
}
