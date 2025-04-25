using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetDetails;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.EventHandlers;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models.Notifications;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetSummary;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Hedging.SpotDeferredTrades.Handlers.Events
{
    public class SpotDeferredTradeCreatedEventHandlerTests
    {
        private readonly Mock<ILogger<SpotDeferredTradeCreatedEventHandler>> _loggerMock;
        private readonly Mock<IRealTimeNotificationPublisher> _publisherMock;
        private readonly Mock<IRepository<SpotDeferredTrade, SpotDeferredTradeId>> _repositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly SpotDeferredTradeCreatedEventHandler _handler;

        public SpotDeferredTradeCreatedEventHandlerTests()
        {
            _loggerMock = new Mock<ILogger<SpotDeferredTradeCreatedEventHandler>>();
            _publisherMock = new Mock<IRealTimeNotificationPublisher>();
            _repositoryMock = new Mock<IRepository<SpotDeferredTrade, SpotDeferredTradeId>>();
            _mediatorMock = new Mock<IMediator>();

            _handler = new SpotDeferredTradeCreatedEventHandler(
                _loggerMock.Object,
                _publisherMock.Object,
                _repositoryMock.Object,
                _mediatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldPublishNotification_WithCorrectData()
        {
            // Arrange
            var spotDeferredTrade = CreateTestSpotDeferredTrade();
            var domainEvent = SpotDeferredTradeCreatedEvent.FromEntity(spotDeferredTrade);
            var notification = new DomainEventNotification<SpotDeferredTradeCreatedEvent>(domainEvent);

            SetupRepositoryAndMediator(spotDeferredTrade);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<SpotDeferredTradeCreatedNotification>>(
                        n => n.Hub == HubType.Hedging &&
                             n.Data.HedgingAccountId == spotDeferredTrade.HedgingAccountId.Value.ToString() &&
                             n.Data.UnrealizedGainOrLossValue == 1000.0m &&
                             n.Data.Items.Count() == 2),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            VerifyLogInformation($"Spot deferred trade created: {spotDeferredTrade.Id}");
        }

        [Fact]
        public async Task Handle_ShouldReturnEarly_WhenFetchingDataFails()
        {
            // Arrange
            var spotDeferredTrade = CreateTestSpotDeferredTrade();
            var domainEvent = SpotDeferredTradeCreatedEvent.FromEntity(spotDeferredTrade);
            var notification = new DomainEventNotification<SpotDeferredTradeCreatedEvent>(domainEvent);

            // Setup repository to return null to simulate a fetch failure
            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(
                    It.IsAny<SpotDeferredTradeId>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<SpotDeferredTrade, object>>[]>()))
                .ReturnsAsync((SpotDeferredTrade?)null);

            // Setup mediator to return account details
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetHedgingAccountDetailsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetHedgingAccountDetailsQueryResult
                {
                    UnrealizedGainOrLossValue = 1000.0m
                });

            // Setup mediator to return trade summary
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetSpotDeferredTradesSummaryQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateTestTradesSummary());

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.IsAny<RealTimeNotification<SpotDeferredTradeCreatedNotification>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            VerifyLogWarning("One or more operations failed when preparing SpotDeferredTradeCreatedNotification. Skipping notification.");
        }

        [Fact]
        public async Task Handle_ShouldReturnEarly_WhenAccountDetailsFetchFails()
        {
            // Arrange
            var spotDeferredTrade = CreateTestSpotDeferredTrade();
            var domainEvent = SpotDeferredTradeCreatedEvent.FromEntity(spotDeferredTrade);
            var notification = new DomainEventNotification<SpotDeferredTradeCreatedEvent>(domainEvent);

            // Setup repository to return the trade
            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(
                    It.IsAny<SpotDeferredTradeId>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<SpotDeferredTrade, object>>[]>()))
                .ReturnsAsync(spotDeferredTrade);

            // Setup mediator to return null account details to simulate a fetch failure
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetHedgingAccountDetailsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetHedgingAccountDetailsQueryResult)null!);

            // Setup mediator to return trade summary
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetSpotDeferredTradesSummaryQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateTestTradesSummary());

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.IsAny<RealTimeNotification<SpotDeferredTradeCreatedNotification>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            VerifyLogWarning("One or more operations failed when preparing SpotDeferredTradeCreatedNotification. Skipping notification.");
        }

        [Fact]
        public async Task Handle_ShouldReturnEarly_WhenTradesSummaryFetchFails()
        {
            // Arrange
            var spotDeferredTrade = CreateTestSpotDeferredTrade();
            var domainEvent = SpotDeferredTradeCreatedEvent.FromEntity(spotDeferredTrade);
            var notification = new DomainEventNotification<SpotDeferredTradeCreatedEvent>(domainEvent);

            // Setup repository to return the trade
            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(
                    It.IsAny<SpotDeferredTradeId>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<SpotDeferredTrade, object>>[]>()))
                .ReturnsAsync(spotDeferredTrade);

            // Setup mediator to return account details
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetHedgingAccountDetailsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetHedgingAccountDetailsQueryResult
                {
                    UnrealizedGainOrLossValue = 1000.0m
                });

            // Setup mediator to return null trade summary to simulate a fetch failure
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetSpotDeferredTradesSummaryQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetSpotDeferredTradesSummaryQueryResult)null!);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.IsAny<RealTimeNotification<SpotDeferredTradeCreatedNotification>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            VerifyLogWarning("One or more operations failed when preparing SpotDeferredTradeCreatedNotification. Skipping notification.");
        }

        [Fact]
        public async Task Handle_ShouldMapItemsCorrectly_WithSingleRelatedTrade()
        {
            // Arrange
            var spotDeferredTrade = CreateTestSpotDeferredTradeWithSingleTrade();
            var domainEvent = SpotDeferredTradeCreatedEvent.FromEntity(spotDeferredTrade);
            var notification = new DomainEventNotification<SpotDeferredTradeCreatedEvent>(domainEvent);

            SetupRepositoryAndMediator(spotDeferredTrade);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<SpotDeferredTradeCreatedNotification>>(
                        n => n.Data.Items.Any(item =>
                            item.TradeReference == "T12345" &&
                            item.TradeType == TradeType.ClientTrade)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldMapItemsCorrectly_WithMultipleRelatedTrades()
        {
            // Arrange
            var spotDeferredTrade = CreateTestSpotDeferredTradeWithMultipleTrades();
            var domainEvent = SpotDeferredTradeCreatedEvent.FromEntity(spotDeferredTrade);
            var notification = new DomainEventNotification<SpotDeferredTradeCreatedEvent>(domainEvent);

            SetupRepositoryAndMediator(spotDeferredTrade);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<SpotDeferredTradeCreatedNotification>>(
                        n => n.Data.Items.Any(item =>
                            item.TradeReference == "N/A" &&
                            item.TradeType == null)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUseCorrectRepository_Parameters()
        {
            // Arrange
            var spotDeferredTrade = CreateTestSpotDeferredTrade();
            var domainEvent = SpotDeferredTradeCreatedEvent.FromEntity(spotDeferredTrade);
            var notification = new DomainEventNotification<SpotDeferredTradeCreatedEvent>(domainEvent);

            SetupRepositoryAndMediator(spotDeferredTrade);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _repositoryMock.Verify(r => r.GetByIdAsync(
                It.Is<SpotDeferredTradeId>(id => id.Equals(spotDeferredTrade.Id)),
                true,
                It.IsAny<CancellationToken>(),
                It.Is<System.Linq.Expressions.Expression<Func<SpotDeferredTrade, object>>[]>(
                    includes => includes.Length == 2)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUseCorrectMediator_Parameters()
        {
            // Arrange
            var spotDeferredTrade = CreateTestSpotDeferredTrade();
            var domainEvent = SpotDeferredTradeCreatedEvent.FromEntity(spotDeferredTrade);
            var notification = new DomainEventNotification<SpotDeferredTradeCreatedEvent>(domainEvent);

            SetupRepositoryAndMediator(spotDeferredTrade);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetHedgingAccountDetailsQuery>(q => q.Id == spotDeferredTrade.HedgingAccountId.Value),
                It.IsAny<CancellationToken>()), Times.Once);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetSpotDeferredTradesSummaryQuery>(q => q.AccountId == spotDeferredTrade.HedgingAccountId.Value),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(SideType.Buy)]
        [InlineData(SideType.Sell)]
        public async Task Handle_WithDifferentSideTypes_ShouldPublishCorrectNotification(SideType sideType)
        {
            // Arrange
            var spotDeferredTrade = CreateTestSpotDeferredTrade(sideType);
            var domainEvent = SpotDeferredTradeCreatedEvent.FromEntity(spotDeferredTrade);
            var notification = new DomainEventNotification<SpotDeferredTradeCreatedEvent>(domainEvent);

            SetupRepositoryAndMediator(spotDeferredTrade);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _publisherMock.Verify(
                p => p.PublishAsync(
                    It.Is<RealTimeNotification<SpotDeferredTradeCreatedNotification>>(
                        n => n.Data.Items.All(item => item.SideType == sideType)),
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

        private void SetupRepositoryAndMediator(SpotDeferredTrade spotDeferredTrade)
        {
            // Setup repository to return the spot deferred trade
            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(
                    It.IsAny<SpotDeferredTradeId>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<SpotDeferredTrade, object>>[]>()))
                .ReturnsAsync(spotDeferredTrade);

            // Setup mediator to return account details
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetHedgingAccountDetailsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetHedgingAccountDetailsQueryResult
                {
                    UnrealizedGainOrLossValue = 1000.0m
                });

            // Setup mediator to return trade summary
            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetSpotDeferredTradesSummaryQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateTestTradesSummary());
        }

        private static SpotDeferredTrade CreateTestSpotDeferredTrade(SideType sideType = SideType.Buy)
        {
            var hedgingAccountId = HedgingAccountId.New();
            var tradeDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var tradeConfirmationReference = "SDT123456";

            var trade = SpotDeferredTrade.Create(
                hedgingAccountId,
                tradeConfirmationReference,
                sideType,
                tradeDate,
                false);

            // Add items for different metal types
            var goldItem = PreciousMetalsTradingSystem.Domain.Entities.SpotDeferredTradeItem.Create(
                MetalType.XAU,
                new Money(1900.0m),
                new QuantityOunces(10.0m));

            var silverItem = PreciousMetalsTradingSystem.Domain.Entities.SpotDeferredTradeItem.Create(
                MetalType.XAG,
                new Money(25.0m),
                new QuantityOunces(100.0m));

            goldItem.SetTrade(trade.Id);
            silverItem.SetTrade(trade.Id);

            trade.AddItem(goldItem);
            trade.AddItem(silverItem);

            // No related trades
            return trade;
        }

        private static SpotDeferredTrade CreateTestSpotDeferredTradeWithSingleTrade()
        {
            var trade = CreateTestSpotDeferredTrade();

            // Add a single related trade
            var relatedTrade = Trade.Create(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.SLC,
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                "Test note");

            // Set the trade number via reflection (since it's calculated internally)
            typeof(Trade).GetProperty("TradeNumber")!.SetValue(relatedTrade, "T12345");

            // Add the trade to the collection
            trade.Trades.Add(relatedTrade);

            return trade;
        }

        private static SpotDeferredTrade CreateTestSpotDeferredTradeWithMultipleTrades()
        {
            var trade = CreateTestSpotDeferredTrade();

            // Add multiple related trades
            var relatedTrade1 = Trade.Create(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.SLC,
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                "Test note 1");

            var relatedTrade2 = Trade.Create(
                TradeType.DealerTrade,
                SideType.Sell,
                LocationType.NY,
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                "Test note 2");

            // Set the trade numbers via reflection
            typeof(Trade).GetProperty("TradeNumber")!.SetValue(relatedTrade1, "T12345");
            typeof(Trade).GetProperty("TradeNumber")!.SetValue(relatedTrade2, "T67890");

            // Add the trades to the collection
            trade.Trades.Add(relatedTrade1);
            trade.Trades.Add(relatedTrade2);

            return trade;
        }

        private static GetSpotDeferredTradesSummaryQueryResult CreateTestTradesSummary()
        {
            var summaryItems = new List<SpotDeferredTradeSummaryItem>
            {
                new()
                {
                    MetalType = MetalType.XAU,
                    ActualTradedBalance = 50.0m,
                    NetAmount = 95000.0m,
                    LastHedgingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5))
                },
                new()
                {
                    MetalType = MetalType.XAG,
                    ActualTradedBalance = 800.0m,
                    NetAmount = 20000.0m,
                    LastHedgingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3))
                }
            };

            return new GetSpotDeferredTradesSummaryQueryResult(summaryItems.AsReadOnly());
        }

        #endregion
    }
}
