using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.SignalR.Hubs
{
    /// <summary>
    /// SignalR hub for inventory notifications
    /// </summary>
    public class InventoryHub : RealTimeNotificationsHub
    {
        /// <summary>
        /// Initializes a new instance of the ProductsHub
        /// </summary>
        /// <param name="logger">Logger for this hub</param>
        public InventoryHub(ILogger<InventoryHub> logger)
            : base(logger)
        {
        }

        // This hub inherits connection handling from the base class
        // You can add activity-specific methods here if needed
    }
}
