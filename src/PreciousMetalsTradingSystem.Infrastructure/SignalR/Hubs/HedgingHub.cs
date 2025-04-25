using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.SignalR.Hubs
{

    /// <summary>
    /// Hub for broadcasting hedging notifications to clients
    /// </summary>
    public class HedgingHub : RealTimeNotificationsHub
    {
        /// <summary>
        /// Initializes a new instance of the HedgingHub
        /// </summary>
        /// <param name="logger">Logger for this hub</param>
        public HedgingHub(ILogger<HedgingHub> logger)
            : base(logger)
        {
        }

        // This hub inherits connection handling from the base class
        // You can add activity-specific methods here if needed
    }
}
