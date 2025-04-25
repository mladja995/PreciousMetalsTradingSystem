using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.Trading.Trades.EventHandlers;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Notifications;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Notifications.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using Microsoft.Extensions.Logging;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Trades.Handlers.Events
{
    public class TradeStateChangedEventsHandlerTests
    {
        private readonly Mock<ILogger<TradeStateChangedEventsHandler>> _loggerMock;
        private readonly Mock<IRealTimeNotificationPublisher> _publisherMock;
        private readonly TradeStateChangedEventsHandler _handler;

        public TradeStateChangedEventsHandlerTests()
        {
            _loggerMock = new Mock<ILogger<TradeStateChangedEventsHandler>>();
            _publisherMock = new Mock<IRealTimeNotificationPublisher>();
            _handler = new TradeStateChangedEventsHandler(_loggerMock.Object, _publisherMock.Object);
        }

        [Fact]
        public async Task Handle_TradeConfirmedEvent_ShouldPublishNotification()
        {
            // Arrange
            var tradeId = new TradeId(Guid.NewGuid());
            var tradeNumber = "220101H123456SLC";
            var confirmedOn = DateTime.UtcNow;

            var domainEvent = new TradeConfirmedEvent(
                tradeId,
                tradeNumber,
                confirmedOn);

            var notification = new DomainEventNotification<TradeConfirmedEvent>(domainEvent);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<TradeStateChangedNotification>>(
                        n => n.Hub == HubType.Activity &&
                             n.Data.Id == tradeId.ToString() &&
                             n.Data.TradeNumber == tradeNumber &&
                             n.Data.ChangeType == TradeStateChangeType.Confirmed &&
                             n.Data.TimestampUtc == confirmedOn),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_TradeFinancialSettledEvent_ShouldPublishNotification()
        {
            // Arrange
            var tradeId = new TradeId(Guid.NewGuid());
            var tradeNumber = "220101H123456NY";
            var settledOn = DateTime.UtcNow;

            var domainEvent = new TradeFinancialSettledEvent(
                tradeId,
                tradeNumber,
                settledOn);

            var notification = new DomainEventNotification<TradeFinancialSettledEvent>(domainEvent);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<TradeStateChangedNotification>>(
                        n => n.Hub == HubType.Activity &&
                             n.Data.Id == tradeId.ToString() &&
                             n.Data.TradeNumber == tradeNumber &&
                             n.Data.ChangeType == TradeStateChangeType.FinancialSettled &&
                             n.Data.TimestampUtc == settledOn),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_TradePositionsSettledEvent_ShouldPublishNotification()
        {
            // Arrange
            var tradeId = new TradeId(Guid.NewGuid());
            var tradeNumber = "220101H123456IDS";
            var settledOn = DateTime.UtcNow;

            var domainEvent = new TradePositionsSettledEvent(
                tradeId,
                tradeNumber,
                settledOn);

            var notification = new DomainEventNotification<TradePositionsSettledEvent>(domainEvent);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<TradeStateChangedNotification>>(
                        n => n.Hub == HubType.Activity &&
                             n.Data.Id == tradeId.ToString() &&
                             n.Data.TradeNumber == tradeNumber &&
                             n.Data.ChangeType == TradeStateChangeType.PositionSettled &&
                             n.Data.TimestampUtc == settledOn),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishTradeStateChangedNotification_ShouldPublishToActivityHub()
        {
            // Arrange
            var tradeId = Guid.NewGuid().ToString();
            var tradeNumber = "220202H234567NY";
            var changeType = TradeStateChangeType.Confirmed;
            var timestamp = DateTime.UtcNow;

            // We'll use reflection to call the protected method
            var method = typeof(TradeStateChangedEventsHandler).GetMethod(
                "PublishTradeStateChangedNotification",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            await (Task)method!.Invoke(_handler,
            [
                tradeId,
                tradeNumber,
                changeType,
                timestamp,
                CancellationToken.None
            ])!;

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<TradeStateChangedNotification>>(
                        n => n.Hub == HubType.Activity &&
                             n.Data.Id == tradeId &&
                             n.Data.TradeNumber == tradeNumber &&
                             n.Data.ChangeType == changeType &&
                             n.Data.TimestampUtc == timestamp),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
