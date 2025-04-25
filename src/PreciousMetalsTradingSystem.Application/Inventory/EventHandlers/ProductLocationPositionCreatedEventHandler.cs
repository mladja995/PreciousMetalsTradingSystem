using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Inventory.Models.Notifications;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Application.Inventory.EventHandlers
{
    /// <summary>
    /// Handles the ProductLocationPositionCreatedEvent
    /// </summary>
    public class ProductLocationPositionCreatedEventHandler :
        SignalRBaseNotificationHandler,
        INotificationHandler<DomainEventNotification<ProductLocationPositionCreatedEvent>>
    {
        private readonly IRepository<Domain.Entities.ProductLocationPosition, ProductLocationPositionId> _positionRepository;

        /// <summary>
        /// Initializes a new instance of the ProductLocationPositionCreatedEventHandler
        /// </summary>
        /// <param name="logger">Logger for this handler</param>
        /// <param name="publisher">Real-time notification publisher</param>
        /// <param name="positionRepository">Repository for position entities</param>
        public ProductLocationPositionCreatedEventHandler(
            ILogger<ProductLocationPositionCreatedEventHandler> logger,
            IRealTimeNotificationPublisher publisher,
            IRepository<Domain.Entities.ProductLocationPosition, ProductLocationPositionId> positionRepository)
            : base(logger, publisher)
        {
            _positionRepository = positionRepository;
        }

        /// <summary>
        /// Handles the ProductLocationPositionCreatedEvent
        /// </summary>
        /// <param name="notification">The event to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task Handle(
            DomainEventNotification<ProductLocationPositionCreatedEvent> notification,
            CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            Logger.LogInformation("Inventory position created: {PositionId} for product: {ProductId}",
                domainEvent.PositionId, domainEvent.ProductId);

            var additionalInfo = await _positionRepository
                .StartQuery(readOnly: true, asSplitQuery: true)
                .Where(x => x.Id.Equals(domainEvent.PositionId))
                .Include(x => x.Product)
                .Include(x => x.Trade)
                .Select(x => new
                {
                    ProductName = x.Product.Name,
                    ProductSKU = x.Product.SKU,
                    TradeNumber = x.Trade.TradeNumber,
                    TradeType = x.Trade.Type
                }).SingleAsync(cancellationToken);

            // Create the notification with relevant data including product and trade info
            var inventoryNotification = new InventoryChangedNotification(
                domainEvent.PositionId.Value.ToString(),
                domainEvent.ProductId.Value.ToString(),
                additionalInfo.ProductName,
                additionalInfo.ProductSKU,
                domainEvent.LocationType,
                domainEvent.SideType,
                domainEvent.Type,
                domainEvent.QuantityUnits,
                domainEvent.PositionUnits,
                domainEvent.TimestampUtc,
                additionalInfo.TradeNumber,
                additionalInfo.TradeType);

            // Publish to the Inventory hub using the base class method
            await PublishNotification(HubType.Inventory, inventoryNotification, cancellationToken);
        }
    }
}
