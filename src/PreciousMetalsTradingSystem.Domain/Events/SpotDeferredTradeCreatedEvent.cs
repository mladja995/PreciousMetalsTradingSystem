using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Event raised when a new SpotDeferredTrade is created
    /// </summary>
    public record SpotDeferredTradeCreatedEvent(
        SpotDeferredTradeId SpotDeferredTradeId,
        HedgingAccountId HedgingAccountId) : DomainEvent(nameof(SpotDeferredTrade))
    {
        /// <summary>
        /// Creates a SpotDeferredTradeCreatedEvent from a SpotDeferredTrade entity
        /// </summary>
        /// <param name="trade">The trade that was created</param>
        /// <returns>A new SpotDeferredTradeCreatedEvent</returns>
        public static SpotDeferredTradeCreatedEvent FromEntity(SpotDeferredTrade trade)
        {
            return new SpotDeferredTradeCreatedEvent(
                trade.Id,
                trade.HedgingAccountId);
        }
    }
}
