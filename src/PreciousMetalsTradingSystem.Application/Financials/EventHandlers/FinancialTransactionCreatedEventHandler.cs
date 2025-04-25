using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Models.Notifications;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Application.Financials.EventHandlers
{
    /// <summary>
    /// Handles the FinancialTransactionCreatedEvent
    /// </summary>
    public class FinancialTransactionCreatedEventHandler :
        SignalRBaseNotificationHandler,
        INotificationHandler<DomainEventNotification<FinancialTransactionCreatedEvent>>
    {
        private readonly IRepository<Domain.Entities.FinancialTransaction, FinancialTransactionId> _transactionRepository;

        /// <summary>
        /// Initializes a new instance of the FinancialTransactionCreatedEventHandler
        /// </summary>
        /// <param name="logger">Logger for this handler</param>
        /// <param name="publisher">Real-time notification publisher</param>
        /// <param name="transactionRepository">Repository for financial transactions</param>
        public FinancialTransactionCreatedEventHandler(
            ILogger<FinancialTransactionCreatedEventHandler> logger,
            IRealTimeNotificationPublisher publisher,
            IRepository<Domain.Entities.FinancialTransaction, FinancialTransactionId> transactionRepository)
            : base(logger, publisher)
        {
            _transactionRepository = transactionRepository;
        }

        /// <summary>
        /// Handles the FinancialTransactionCreatedEvent
        /// </summary>
        /// <param name="notification">The notification containing the event</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task Handle(
            DomainEventNotification<FinancialTransactionCreatedEvent> notification,
            CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            Logger.LogInformation("Financial transaction created: {TransactionId} for balance type: {BalanceType}",
                domainEvent.TransactionId, domainEvent.BalanceType);

            // Get additional information about the transaction
            var additionalInfo = await _transactionRepository
                .StartQuery(readOnly: true, asSplitQuery: true)
                .Where(x => x.Id.Equals(domainEvent.TransactionId))
                .Include(x => x.Trade)
                .Include(x => x.FinancialAdjustment)
                .Select(x => new
                {
                    Reference = x.Trade != null ? x.Trade.TradeNumber : string.Empty,
                    IsFinancialSettled = x.Trade == null || x.Trade.IsFinancialSettled,
                    Note = x.FinancialAdjustment != null ? x.FinancialAdjustment.Note : null
                })
                .SingleAsync(cancellationToken);

            // Create the notification with financial transaction details
            var balanceNotification = new FinancialBalanceChangedNotification(
                domainEvent.TransactionId.Value.ToString(),
                domainEvent.TimestampUtc,
                domainEvent.BalanceType,
                domainEvent.SideType,
                domainEvent.ActivityType,
                domainEvent.Amount,
                domainEvent.Balance,
                additionalInfo.Reference,
                additionalInfo.IsFinancialSettled,
                additionalInfo.Note ?? string.Empty);

            // Publish to the Financials hub
            await PublishNotification(HubType.Financials, balanceNotification, cancellationToken);
        }
    }
}
