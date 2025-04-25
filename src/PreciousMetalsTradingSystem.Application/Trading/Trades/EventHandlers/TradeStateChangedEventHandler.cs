using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models.Notifications;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Notifications;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Notifications.Enums;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.EventHandlers
{
    /// <summary>
    /// Handles the TradeStateChanged
    /// </summary>
    public class TradeStateChangedEventsHandler :
        SignalRBaseNotificationHandler,
         INotificationHandler<DomainEventNotification<TradeConfirmedEvent>>,
         INotificationHandler<DomainEventNotification<TradeFinancialSettledEvent>>,
         INotificationHandler<DomainEventNotification<TradePositionsSettledEvent>>
    {
        /// <summary>
        /// Initializes a new instance of the TradeStateChangedEventHandler
        /// </summary>
        /// <param name="hubContext">SignalR hub context</param>
        /// <param name="logger">Logger for this handler</param>
        public TradeStateChangedEventsHandler(
            ILogger<TradeStateChangedEventsHandler> logger,
            IRealTimeNotificationPublisher publisher) 
            : base (logger, publisher) {}
        /// <summary>
        /// Handles the TradeConfirmedEvent
        /// </summary>
        /// <param name="notification">The event to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task Handle(DomainEventNotification<TradeConfirmedEvent> notification, CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            await PublishTradeStateChangedNotification(
                        domainEvent.Id.ToString(),
                        domainEvent.TradeNumber,
                        TradeStateChangeType.Confirmed,
                        domainEvent.ConfirmedOnUtc,
                        cancellationToken);

        }

        /// <summary>
        /// Handles the TradeFinancialSettledEvent
        /// </summary>
        /// <param name="notification">The event to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task Handle(DomainEventNotification<TradeFinancialSettledEvent> notification, CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            await PublishTradeStateChangedNotification(
                            domainEvent.Id.ToString(),
                            domainEvent.TradeNumber,
                            TradeStateChangeType.FinancialSettled,
                            domainEvent.SettledOnUtc,
                            cancellationToken);
        }

        /// <summary>
        /// Handles the TradeFinancialSettledEvent
        /// </summary>
        /// <param name="notification">The event to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task Handle(DomainEventNotification<TradePositionsSettledEvent> notification, CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            await PublishTradeStateChangedNotification(
                        domainEvent.Id.ToString(),
                        domainEvent.TradeNumber,
                        TradeStateChangeType.PositionSettled,
                        domainEvent.SettledOnUtc,
                        cancellationToken);

        }
        /// <summary>
        /// Publishes a trade state changed notification with the given data
        /// </summary>
        protected async Task PublishTradeStateChangedNotification(
            string tradeId,
            string tradeNumber,
            TradeStateChangeType tradeStateChangeType,
            DateTime timestampUtc,
            CancellationToken cancellationToken)
        {
            var notification = new TradeStateChangedNotification(
                tradeId,
                tradeNumber,
                tradeStateChangeType,
                timestampUtc);

            await PublishNotification(HubType.Activity, notification, cancellationToken);
        }
    }
}