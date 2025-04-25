namespace PreciousMetalsTradingSystem.Application.Common.Notifications
{
    /// <summary>
    /// Base class for real-time notifications
    /// </summary>
    /// <typeparam name="T">The type of data contained in the notification</typeparam>
    public sealed class RealTimeNotification<T>
    {
        /// <summary>
        /// Gets the hub this notification should be sent to
        /// </summary>
        public HubType Hub { get; }

        /// <summary>
        /// Gets the method name to invoke on the client
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Gets the data to send with the notification
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Initializes a new instance of the RealTimeNotification class
        /// </summary>
        /// <param name="hub">The hub to send this notification to</param>
        /// <param name="data">The data for this notification</param>
        public RealTimeNotification(HubType hub, T data)
        {
            Hub = hub;
            Data = data;
            MethodName = typeof(T).Name;
        }
    }
}
