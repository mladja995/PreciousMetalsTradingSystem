using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetDetails;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Models.Notifications;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.Extensions.Logging;
using HedgingItem = PreciousMetalsTradingSystem.Domain.Entities.HedgingItem;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.EventHandlers
{
    public class HedgingItemEventsHandler : SignalRBaseNotificationHandler,
        INotificationHandler<DomainEventNotification<HedgingItemCreatedEvent>>,
        INotificationHandler<DomainEventNotification<HedgingItemUpdatedEvent>>,
        INotificationHandler<DomainEventNotification<HedgingItemDeletedEvent>>
    {
        private readonly IRepository<HedgingItem, HedgingItemId> _repositoryHedgingItem;
        private readonly IMediator _mediator;

        public HedgingItemEventsHandler(
            ILogger<HedgingItemEventsHandler> logger,
            IRealTimeNotificationPublisher publisher,
            IRepository<HedgingItem, HedgingItemId> repositoryHedgingItem,
            IMediator mediator)
            : base(logger, publisher)
        {
            _repositoryHedgingItem = repositoryHedgingItem;
            _mediator = mediator;
        }

        /// <summary>
        /// Handles the HedgingItemCreatedEvent
        /// </summary>
        /// <param name="notification">The event to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// 
        public async Task Handle(DomainEventNotification<HedgingItemCreatedEvent> notification, CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            Logger.LogInformation("HedgingItem created: {HedgingItemId}", domainEvent.HedgingItemId);

            // Fetch all required data
            var data = await FetchRequiredDataAsync(domainEvent.HedgingAccountId, domainEvent.HedgingItemId, cancellationToken);
            if (data == null)
            {
                return; // Error occurred, warning already logged
            }

            await PublishHedgingItemNotification(
                domainEvent.HedgingItemId.Value.ToString(),
                domainEvent.HedgingAccountId.Value.ToString(),
                data.Value.HedgingItem.HedgingItemDate,
                data.Value.HedgingItem.Type,
                data.Value.HedgingItem.SideType,
                data.Value.HedgingItem.Note ?? string.Empty,
                data.Value.HedgingItem.Amount,
                data.Value.AccountDetails.UnrealizedGainOrLossValue,
                ChangeType.Created,
                cancellationToken);
        }

        /// <summary>
        /// Handles the HedgingItemUpdatedEvent
        /// </summary>
        /// <param name="notification">The event to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task Handle(DomainEventNotification<HedgingItemUpdatedEvent> notification, CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            Logger.LogInformation("HedgingItem updated: {HedgingItemId}", domainEvent.HedgingItemId);

            // Fetch all required data
            var data = await FetchRequiredDataAsync(domainEvent.HedgingAccountId, domainEvent.HedgingItemId, cancellationToken);
            if (data == null)
            {
                return; // Error occurred, warning already logged
            }

            await PublishHedgingItemNotification(
               domainEvent.HedgingItemId.Value.ToString(),
               domainEvent.HedgingAccountId.Value.ToString(),
               data.Value.HedgingItem.HedgingItemDate,
               data.Value.HedgingItem.Type,
               data.Value.HedgingItem.SideType,
               data.Value.HedgingItem.Note ?? string.Empty,
               data.Value.HedgingItem.Amount,
               data.Value.AccountDetails.UnrealizedGainOrLossValue,
               ChangeType.Updated,
               cancellationToken);
        }

        /// <summary>
        /// Handles the HedgingItemDeletedEvent
        /// </summary>
        /// <param name="notification">The event to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task Handle(DomainEventNotification<HedgingItemDeletedEvent> notification, CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            Logger.LogInformation("HedgingItem deleted: {HedgingItemId}", domainEvent.HedgingItemId);

            var accountDetails = await _mediator.Send(
                new GetHedgingAccountDetailsQuery { Id = domainEvent.HedgingAccountId },
                cancellationToken);

            await PublishHedgingItemNotification(
               domainEvent.HedgingItemId.Value.ToString(),
               domainEvent.HedgingAccountId.Value.ToString(),
               domainEvent.HedgingItemDate,
               domainEvent.Type,
               domainEvent.SideType,
               domainEvent.Note,
               domainEvent.Amount,
               accountDetails.UnrealizedGainOrLossValue,
               ChangeType.Deleted,
               cancellationToken);
        }

        private async Task PublishHedgingItemNotification(
            string hedgingItemId,
            string hedgingAccountId,
            DateOnly date,
            HedgingItemType type,
            HedgingItemSideType sideType,
            string note,
            decimal amount,
            decimal unrealizedGainOrLossValue,
            ChangeType changeType,
            CancellationToken cancellationToken)
        {
            var notification = new HedgingItemChangedNotification(
                hedgingAccountId,
                hedgingItemId,
                changeType,
                date,
                type,
                sideType,
                amount,
                note,
                unrealizedGainOrLossValue
                );

            await PublishNotification(HubType.Hedging, notification, cancellationToken);
            
        }

        private async Task<(GetHedgingAccountDetailsQueryResult AccountDetails, HedgingItem HedgingItem)?> 
            FetchRequiredDataAsync(
                HedgingAccountId hedgingAccountId, 
                HedgingItemId hedgingItemId, 
                CancellationToken cancellationToken)
        {
            var accountDetails = await _mediator.Send(
                new GetHedgingAccountDetailsQuery { Id = hedgingAccountId.Value },
                cancellationToken);

            var hedgingItem = await _repositoryHedgingItem.GetByIdAsync(
                id: hedgingItemId,
                readOnly: true,
                cancellationToken: cancellationToken
            );


            if (accountDetails == default || hedgingItem is null)
            {
                Logger.LogWarning("One or more operations failed when preparing HedgingItem notification. Skipping notification.");
                return null;
            }

            return (accountDetails, hedgingItem);
        }


    }
}
