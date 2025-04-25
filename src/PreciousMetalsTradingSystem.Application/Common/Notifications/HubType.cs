namespace PreciousMetalsTradingSystem.Application.Common.Notifications
{
    /// <summary>
    /// Defines the types of notification hubs in the system
    /// </summary>
    public enum HubType
    {
        /// <summary>
        /// Hub for activity notifications
        /// </summary>
        Activity = 1,

        /// <summary>
        /// Hub for products notifications
        /// </summary>
        Products,

        /// <summary>
        /// Hub for inventory notifications
        /// </summary>
        Inventory,

        /// <summary>
        /// Hub for hedging notifications
        /// </summary>
        Hedging,

        /// <summary>
        /// Hub for financials notifications
        /// </summary>
        Financials,

        // Add more hub types as needed
    }
}
