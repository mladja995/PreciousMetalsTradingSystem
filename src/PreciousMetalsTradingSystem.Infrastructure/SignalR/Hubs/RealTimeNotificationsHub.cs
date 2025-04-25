using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.SignalR.Hubs
{
    /// <summary>
    /// Abstract base class for all real-time notification hubs
    /// </summary>
    [Authorize]
    public abstract class RealTimeNotificationsHub : Hub
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RealTimeNotificationsHub
        /// </summary>
        /// <param name="logger">Logger for the hub</param>
        protected RealTimeNotificationsHub(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles client connection
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name ?? "Anonymous";
            var isAuthenticated = Context.User?.Identity?.IsAuthenticated ?? false;

            if (isAuthenticated)
            {
                _logger.LogInformation(
                    "Client connected: {ConnectionId} (User: {UserId})",
                    Context.ConnectionId,
                    userId);
            }
            else
            {
                _logger.LogWarning(
                    "Unauthenticated client connected: {ConnectionId}",
                    Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Handles client disconnection
        /// </summary>
        /// <param name="exception">The exception that caused the disconnection, if any</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.Identity?.Name ?? "Anonymous";

            if (exception != null)
            {
                _logger.LogWarning(
                    exception,
                    "Client disconnected with error: {ConnectionId} (User: {UserId})",
                    Context.ConnectionId,
                    userId);
            }
            else
            {
                _logger.LogInformation(
                    "Client disconnected: {ConnectionId} (User: {UserId})",
                    Context.ConnectionId,
                    userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
