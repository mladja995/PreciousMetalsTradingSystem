using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Infrastructure.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.SignalR
{
    /// <summary>
    /// SignalR implementation of the real-time notification publisher
    /// </summary>
    public class SignalRNotificationPublisher : IRealTimeNotificationPublisher
    {
        private readonly IHubContext<ActivityHub> _activityHubContext;
        private readonly IHubContext<ProductsHub> _productsHubContext;
        private readonly IHubContext<FinancialsHub> _financialsHubContext;
        private readonly IHubContext<InventoryHub> _inventoryHubContext;
        private readonly IHubContext<HedgingHub> _hedgingHubContext;
        // Add other hub contexts as needed
        private readonly ILogger<SignalRNotificationPublisher> _logger;

        private readonly Dictionary<HubType, IHubContext<RealTimeNotificationsHub>> _hubContexts;

        /// <summary>
        /// Initializes a new instance of the SignalRNotificationPublisher
        /// </summary>
        public SignalRNotificationPublisher(
            IHubContext<ActivityHub> activityHubContext,
            IHubContext<ProductsHub> productsHubContext,
            IHubContext<FinancialsHub> financialsHubContext,
            IHubContext<InventoryHub> inventoryHubContext,
            IHubContext<HedgingHub> hedgingHubContext,
            // Add other hub contexts as needed
            ILogger<SignalRNotificationPublisher> logger)
        {
            _activityHubContext = activityHubContext;
            _productsHubContext = productsHubContext;
            _financialsHubContext = financialsHubContext;
            _inventoryHubContext = inventoryHubContext;
            _hedgingHubContext = hedgingHubContext;
            _logger = logger;

            // Map hub types to their contexts
            _hubContexts = new Dictionary<HubType, IHubContext<RealTimeNotificationsHub>>
            {
                { HubType.Activity, _activityHubContext },
                { HubType.Products, _productsHubContext },
                { HubType.Financials, _financialsHubContext },
                { HubType.Inventory, _inventoryHubContext },
                { HubType.Hedging, _hedgingHubContext },

                // Add other hubs as needed
            };
        }

        /// <summary>
        /// Publishes a notification to connected clients
        /// </summary>
        public async Task PublishAsync<T>(RealTimeNotification<T> notification, CancellationToken cancellationToken = default)
        {
            if (!_hubContexts.TryGetValue(notification.Hub, out var hubContext))
            {
                _logger.LogWarning("Hub '{HubType}' not found for notification", notification.Hub);
                return;
            }

            try
            {
                _logger.LogDebug("Publishing {MethodName} notification to {HubType} hub",
                    notification.MethodName, notification.Hub);

                await hubContext.Clients.All.SendAsync(
                    notification.MethodName, notification.Data, cancellationToken);

                _logger.LogDebug("Successfully published notification to {HubType}Hub.{MethodName}",
                    notification.Hub, notification.MethodName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing notification to {HubType}Hub.{MethodName}",
                    notification.Hub, notification.MethodName);

                // Note: We're catching the exception but not rethrowing it
            }
        }
    }
}
