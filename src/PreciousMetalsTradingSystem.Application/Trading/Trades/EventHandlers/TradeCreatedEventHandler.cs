using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Notifications;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.EventHandlers
{
    /// <summary>
    /// Handles the TradeCreatedEvent
    /// </summary>
    public class TradeCreatedEventHandler :
        SignalRBaseNotificationHandler,
        INotificationHandler<DomainEventNotification<TradeCreatedEvent>>
    {
        private readonly IRepository<Domain.Entities.Trade, TradeId> _tradeRepository;

        /// <summary>
        /// Initializes a new instance of the TradeCreatedEventHandler
        /// </summary>
        /// <param name="logger">Logger for this handler</param>
        /// <param name="publisher">Real-time notification publisher</param>
        /// <param name="tradeRepository">Repository for trade entities</param>
        public TradeCreatedEventHandler(
            ILogger<TradeCreatedEventHandler> logger,
            IRealTimeNotificationPublisher publisher,
            IRepository<Domain.Entities.Trade, TradeId> tradeRepository)
            : base(logger, publisher)
        {
            _tradeRepository = tradeRepository;
        }

        /// <summary>
        /// Handles the TradeCreatedEvent
        /// </summary>
        /// <param name="notification">The notification containing the event</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task Handle(
            DomainEventNotification<TradeCreatedEvent> notification,
            CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            Logger.LogInformation("Trade created: {TradeId}, TradeNumber: {TradeNumber}",
                domainEvent.TradeId, domainEvent.TradeNumber);

            // Fetch the complete trade with items and products
            var trade = await _tradeRepository.StartQuery(readOnly: true, asSplitQuery: true)
                .Where(t => t.Id.Equals(domainEvent.TradeId))
                .Include(t => t.Items)
                .ThenInclude(i => i.Product)
                .SingleAsync(cancellationToken);

            // Map trade items to notification items
            var items = trade.Items.Select(item => new TradeItemData(
                TradeItemId: item.Id.Value.ToString(),
                ProductId: item.ProductId.Value.ToString(),
                ProductName: item.Product.Name,
                ProductSKU: item.Product.SKU,
                QuantityUnits: item.QuantityUnits,
                SpotPricePerOz: item.SpotPricePerOz,
                TradePricePerOz: item.TradePricePerOz,
                PremiumPerOz: item.PremiumPerOz,
                EffectivePricePerOz: item.EffectivePricePerOz,
                TotalRevenue: item.TotalRevenue,
                TotalEffectivePrice: item.TotalEffectivePrice
            ));

            // Create the trade notification
            var tradeNotification = new TradeCreatedNotification(
                TradeId: trade.Id.Value.ToString(),
                TradeNumber: trade.TradeNumber,
                TradeType: trade.Type,
                SideType: trade.Side,
                LocationType: trade.LocationType,
                TradeDate: trade.TradeDate,
                TimestampUtc: trade.TimestampUtc,
                Note: trade.Note ?? string.Empty,
                IsPositionSettled: trade.IsPositionSettled,
                PositionSettledOnUtc: trade.PositionSettledOnUtc,
                IsFinancialSettled: trade.IsFinancialSettled,
                FinancialSettledOnUtc: trade.FinancialSettledOnUtc,
                ConfirmedOnUtc: trade.ConfirmedOnUtc,
                Items: items);

            // Publish to the Activity hub
            await PublishNotification(HubType.Activity, tradeNotification, cancellationToken);
        }
    }
}
