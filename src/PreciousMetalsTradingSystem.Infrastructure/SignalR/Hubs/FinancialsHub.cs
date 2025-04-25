using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.SignalR.Hubs
{
    /// <summary>
    /// Hub for broadcasting financials notifications to clients
    /// </summary>
    public class FinancialsHub : RealTimeNotificationsHub
    {
        /// <summary>
        /// Initializes a new instance of the FinancialsHub
        /// </summary>
        /// <param name="logger">Logger for this hub</param>
        public FinancialsHub(ILogger<FinancialsHub> logger)
            : base(logger)
        {
        }

        // This hub inherits connection handling from the base class
        // You can add activity-specific methods here if needed
    }
}
