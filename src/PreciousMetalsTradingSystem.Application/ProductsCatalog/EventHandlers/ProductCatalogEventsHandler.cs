using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models.Notifications;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.EventHandlers
{
    /// <summary>
    /// Handles product-related domain events
    /// </summary>
    public class ProductCatalogEventsHandler :
        SignalRBaseNotificationHandler,
        INotificationHandler<DomainEventNotification<ProductCreatedEvent>>,
        INotificationHandler<DomainEventNotification<ProductUpdatedEvent>>
    {
        /// <summary>
        /// Initializes a new instance of the ProductEventsHandler
        /// </summary>
        /// <param name="logger">Logger for this handler</param>
        /// <param name="publisher">Real-time notification publisher</param>
        public ProductCatalogEventsHandler(
            ILogger<ProductCatalogEventsHandler> logger,
            IRealTimeNotificationPublisher publisher)
            : base(logger, publisher)
        {
        }

        /// <summary>
        /// Handles the ProductCreatedEvent
        /// </summary>
        /// <param name="notification">The event to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task Handle(
            DomainEventNotification<ProductCreatedEvent> notification,
            CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            Logger.LogInformation("Product created: {ProductId}", domainEvent.ProductId);

            await PublishProductNotification(
                domainEvent.ProductId.Value.ToString(),
                domainEvent.Name,
                domainEvent.SKU,
                domainEvent.WeightInOz,
                domainEvent.MetalType,
                domainEvent.IsAvailable,
                MapLocationConfigurations(domainEvent.LocationConfigurations),
                ChangeType.Created,
                cancellationToken);
        }

        /// <summary>
        /// Handles the ProductUpdatedEvent
        /// </summary>
        /// <param name="notification">The event to handle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task Handle(
            DomainEventNotification<ProductUpdatedEvent> notification,
            CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            Logger.LogInformation("Product updated: {ProductId}", domainEvent.ProductId);

            await PublishProductNotification(
                domainEvent.ProductId.Value.ToString(),
                domainEvent.Name,
                domainEvent.SKU,
                domainEvent.WeightInOz,
                domainEvent.MetalType,
                domainEvent.IsAvailable,
                MapLocationConfigurations(domainEvent.LocationConfigurations),
                ChangeType.Updated,
                cancellationToken);
        }

        /// <summary>
        /// Publishes a product notification with the given data
        /// </summary>
        private async Task PublishProductNotification(
            string productId,
            string name,
            string sku,
            decimal weightInOz,
            MetalType metalType,
            bool isAvailable,
            IEnumerable<LocationConfigurationData> locationConfigurations,
            ChangeType changeType,
            CancellationToken cancellationToken)
        {
            var notification = new ProductCatalogChangedNotification(
                productId,
                changeType,
                name,
                sku,
                weightInOz,
                metalType,
                isAvailable,
                locationConfigurations);

            await PublishNotification(HubType.Products, notification, cancellationToken);
        }

        /// <summary>
        /// Maps domain event location configurations to notification location configurations
        /// </summary>
        private static IEnumerable<LocationConfigurationData> MapLocationConfigurations(
            IReadOnlyCollection<ProductCreatedEvent.LocationConfigurationData> configurations)
        {
            return configurations.Select(config => new LocationConfigurationData(
                config.LocationType.ToString(),
                config.PremiumUnitType.ToString(),
                config.BuyPremium.Value,
                config.SellPremium.Value,
                config.IsAvailableForBuy,
                config.IsAvailableForSell));
        }

        /// <summary>
        /// Maps domain event location configurations to notification location configurations
        /// </summary>
        private static IEnumerable<LocationConfigurationData> MapLocationConfigurations(
            IReadOnlyCollection<ProductUpdatedEvent.LocationConfigurationData> configurations)
        {
            return configurations.Select(config => new LocationConfigurationData(
                config.LocationType.ToString(),
                config.PremiumUnitType.ToString(),
                config.BuyPremium.Value,
                config.SellPremium.Value,
                config.IsAvailableForBuy,
                config.IsAvailableForSell));
        }
    }
}
