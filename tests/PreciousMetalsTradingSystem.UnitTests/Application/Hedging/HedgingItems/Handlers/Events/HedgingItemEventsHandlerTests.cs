using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetDetails;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.EventHandlers;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Models.Notifications;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Hedging.HedgingItems.Handlers.Events
{
    public class HedgingItemEventsHandlerTests
    {
        private readonly Mock<ILogger<HedgingItemEventsHandler>> _loggerMock;
        private readonly Mock<IRealTimeNotificationPublisher> _publisherMock;
        private readonly Mock<IRepository<HedgingItem, HedgingItemId>> _repositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly HedgingItemEventsHandler _handler;

        public HedgingItemEventsHandlerTests()
        {
            _loggerMock = new Mock<ILogger<HedgingItemEventsHandler>>();
            _publisherMock = new Mock<IRealTimeNotificationPublisher>();
            _repositoryMock = new Mock<IRepository<HedgingItem, HedgingItemId>>();
            _mediatorMock = new Mock<IMediator>();

            _handler = new HedgingItemEventsHandler(
                _loggerMock.Object,
                _publisherMock.Object,
                _repositoryMock.Object,
                _mediatorMock.Object);
        }

        [Fact]
        public async Task Handle_HedgingItemCreatedEvent_ShouldPublishNotification_WithCorrectData()
        {
            // Arrange
            var hedgingItem = CreateTestHedgingItem();
            var domainEvent = HedgingItemCreatedEvent.FromEntity(hedgingItem);
            var notification = new DomainEventNotification<HedgingItemCreatedEvent>(domainEvent);

            SetupRepositoryAndMediator(hedgingItem);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<HedgingItemChangedNotification>>(
                        n => n.Hub == HubType.Hedging &&
                             n.Data.HedgingItemId == hedgingItem.Id.Value.ToString() &&
                             n.Data.HedgingAccountId == hedgingItem.HedgingAccountId.Value.ToString() &&
                             n.Data.ChangeType == ChangeType.Created &&
                             n.Data.Date == hedgingItem.HedgingItemDate &&
                             n.Data.HedgingItemType == hedgingItem.Type &&
                             n.Data.HedgingItemSideType == hedgingItem.SideType &&
                             n.Data.Amount == hedgingItem.Amount.Value &&
                             n.Data.Note == (hedgingItem.Note ?? string.Empty) &&
                             n.Data.UnrealizedGainOrLossValue == 1000.0m),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            VerifyLogInformation($"HedgingItem created: {hedgingItem.Id}");
        }

        [Fact]
        public async Task Handle_HedgingItemCreatedEvent_ShouldReturnEarly_WhenFetchingDataFails()
        {
            // Arrange
            var hedgingItem = CreateTestHedgingItem();
            var domainEvent = HedgingItemCreatedEvent.FromEntity(hedgingItem);
            var notification = new DomainEventNotification<HedgingItemCreatedEvent>(domainEvent);

            // Setup repository to return null to simulate a fetch failure
            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(
                    It.IsAny<HedgingItemId>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<HedgingItem, object>>[]>()))
                .ReturnsAsync((HedgingItem?)null);

            // Setup mediator to return account details
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetHedgingAccountDetailsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetHedgingAccountDetailsQueryResult
                {
                    UnrealizedGainOrLossValue = 1000.0m
                });

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.IsAny<RealTimeNotification<HedgingItemChangedNotification>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            VerifyLogWarning("One or more operations failed when preparing HedgingItem notification. Skipping notification.");
        }

        [Fact]
        public async Task Handle_HedgingItemUpdatedEvent_ShouldPublishNotification_WithCorrectData()
        {
            // Arrange
            var hedgingItem = CreateTestHedgingItem();
            var domainEvent = HedgingItemUpdatedEvent.FromEntity(hedgingItem);
            var notification = new DomainEventNotification<HedgingItemUpdatedEvent>(domainEvent);

            SetupRepositoryAndMediator(hedgingItem);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<HedgingItemChangedNotification>>(
                        n => n.Hub == HubType.Hedging &&
                             n.Data.HedgingItemId == hedgingItem.Id.Value.ToString() &&
                             n.Data.HedgingAccountId == hedgingItem.HedgingAccountId.Value.ToString() &&
                             n.Data.ChangeType == ChangeType.Updated &&
                             n.Data.Date == hedgingItem.HedgingItemDate &&
                             n.Data.HedgingItemType == hedgingItem.Type &&
                             n.Data.HedgingItemSideType == hedgingItem.SideType &&
                             n.Data.Amount == hedgingItem.Amount.Value &&
                             n.Data.Note == (hedgingItem.Note ?? string.Empty) &&
                             n.Data.UnrealizedGainOrLossValue == 1000.0m),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            VerifyLogInformation($"HedgingItem updated: {hedgingItem.Id}");
        }

        [Fact]
        public async Task Handle_HedgingItemDeletedEvent_ShouldPublishNotification_WithCorrectData()
        {
            // Arrange
            var hedgingItem = CreateTestHedgingItem();
            var domainEvent = HedgingItemDeletedEvent.FromEntity(hedgingItem);
            var notification = new DomainEventNotification<HedgingItemDeletedEvent>(domainEvent);

            // For deleted event, we don't need to fetch the hedging item from repository
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetHedgingAccountDetailsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetHedgingAccountDetailsQueryResult
                {
                    UnrealizedGainOrLossValue = 1000.0m
                });

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<HedgingItemChangedNotification>>(
                        n => n.Hub == HubType.Hedging &&
                             n.Data.HedgingItemId == hedgingItem.Id.Value.ToString() &&
                             n.Data.HedgingAccountId == hedgingItem.HedgingAccountId.Value.ToString() &&
                             n.Data.ChangeType == ChangeType.Deleted &&
                             n.Data.Date == hedgingItem.HedgingItemDate &&
                             n.Data.HedgingItemType == hedgingItem.Type &&
                             n.Data.HedgingItemSideType == hedgingItem.SideType &&
                             n.Data.Amount == hedgingItem.Amount.Value &&
                             n.Data.Note == (hedgingItem.Note ?? string.Empty) &&
                             n.Data.UnrealizedGainOrLossValue == 1000.0m),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            VerifyLogInformation($"HedgingItem deleted: {hedgingItem.Id}");
        }

        [Theory]
        [InlineData(HedgingItemType.ProfitLosses, HedgingItemSideType.WireIn)]
        [InlineData(HedgingItemType.MonthlyFee, HedgingItemSideType.WireOut)]
        public async Task Handle_HedgingItemCreatedEvent_WithDifferentTypesAndSides_ShouldPublishCorrectNotification(
            HedgingItemType itemType,
            HedgingItemSideType sideType)
        {
            // Arrange
            var hedgingItem = CreateTestHedgingItem(itemType, sideType);
            var domainEvent = HedgingItemCreatedEvent.FromEntity(hedgingItem);
            var notification = new DomainEventNotification<HedgingItemCreatedEvent>(domainEvent);

            SetupRepositoryAndMediator(hedgingItem);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<HedgingItemChangedNotification>>(
                        n => n.Data.HedgingItemType == itemType &&
                             n.Data.HedgingItemSideType == sideType),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUseCorrectRepository_Parameters()
        {
            // Arrange
            var hedgingItem = CreateTestHedgingItem();
            var domainEvent = HedgingItemCreatedEvent.FromEntity(hedgingItem);
            var notification = new DomainEventNotification<HedgingItemCreatedEvent>(domainEvent);

            SetupRepositoryAndMediator(hedgingItem);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _repositoryMock.Verify(r => r.GetByIdAsync(
                It.Is<HedgingItemId>(id => id.Equals(hedgingItem.Id)),
                true,
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<HedgingItem, object>>[]>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUseCorrectMediator_Parameters()
        {
            // Arrange
            var hedgingItem = CreateTestHedgingItem();
            var domainEvent = HedgingItemCreatedEvent.FromEntity(hedgingItem);
            var notification = new DomainEventNotification<HedgingItemCreatedEvent>(domainEvent);

            SetupRepositoryAndMediator(hedgingItem);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetHedgingAccountDetailsQuery>(q => q.Id == hedgingItem.HedgingAccountId.Value),
                It.IsAny<CancellationToken>()), Times.Once);
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

        private void VerifyLogWarning(string expectedMessage)
        {
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains(expectedMessage)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void SetupRepositoryAndMediator(HedgingItem hedgingItem)
        {
            // Setup repository to return the hedging item
            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(
                    It.IsAny<HedgingItemId>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<HedgingItem, object>>[]>()))
                .ReturnsAsync(hedgingItem);

            // Setup mediator to return account details
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetHedgingAccountDetailsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetHedgingAccountDetailsQueryResult
                {
                    UnrealizedGainOrLossValue = 1000.0m
                });
        }

        private static HedgingItem CreateTestHedgingItem(
            HedgingItemType type = HedgingItemType.ProfitLosses,
            HedgingItemSideType sideType = HedgingItemSideType.WireIn,
            string? note = "Test note")
        {
            var hedgingAccountId = HedgingAccountId.New();
            var date = DateOnly.FromDateTime(DateTime.UtcNow);
            var amount = new Money(500.0m);

            return HedgingItem.Create(
                hedgingAccountId,
                date,
                type,
                sideType,
                amount,
                note);
        }

        #endregion
    }
}
