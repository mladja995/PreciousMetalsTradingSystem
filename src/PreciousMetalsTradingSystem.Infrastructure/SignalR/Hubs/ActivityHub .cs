using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.SignalR.Hubs
{
    /// <summary>
    /// Hub for broadcasting activity notifications to clients
    /// </summary>
    public class ActivityHub : RealTimeNotificationsHub
    {
        /// <summary>
        /// Initializes a new instance of the ActivityHub
        /// </summary>
        /// <param name="logger">Logger for this hub</param>
        public ActivityHub(ILogger<ActivityHub> logger)
            : base(logger)
        {
        }

        // This hub inherits connection handling from the base class
        // You can add activity-specific methods here if needed
    }
}
