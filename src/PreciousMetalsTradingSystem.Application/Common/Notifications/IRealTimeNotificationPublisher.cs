namespace PreciousMetalsTradingSystem.Application.Common.Notifications
{
    /// <summary>
    /// Service for publishing real-time notifications
    /// </summary>
    public interface IRealTimeNotificationPublisher
    {
        /// <summary>
        /// Publishes a notification to connected clients
        /// </summary>
        /// <typeparam name="T">The type of the notification data</typeparam>
        /// <param name="notification">The notification to publish</param>
        Task PublishAsync<T>(RealTimeNotification<T> notification, CancellationToken cancellationToken = default);
    }
}
