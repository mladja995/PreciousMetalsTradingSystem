using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Application.Common.Notifications
{
    /// <summary>
    /// Base class for handlers that publish real-time notifications
    /// </summary>
    public abstract class SignalRBaseNotificationHandler
    {
        /// <summary>
        /// Logger for this handler
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Publisher for real-time notifications
        /// </summary>
        protected readonly IRealTimeNotificationPublisher Publisher;

        /// <summary>
        /// Initializes a new instance of the BaseNotificationHandler
        /// </summary>
        /// <param name="logger">Logger for this handler</param>
        /// <param name="publisher">Publisher for real-time notifications</param>
        protected SignalRBaseNotificationHandler(
            ILogger logger,
            IRealTimeNotificationPublisher publisher)
        {
            Logger = logger;
            Publisher = publisher;
        }

        /// <summary>
        /// Helper method to publish a notification
        /// </summary>
        /// <typeparam name="TNotification">The type of notification to publish</typeparam>
        /// <param name="hubType">The hub to publish to</param>
        /// <param name="notification">The notification data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected async Task PublishNotification<TNotification>(
            HubType hubType,
            TNotification notification,
            CancellationToken cancellationToken)
        {
            var realTimeNotification = new RealTimeNotification<TNotification>(
                hubType,
                notification);

            await Publisher.PublishAsync(realTimeNotification, cancellationToken);
        }
    }
}
