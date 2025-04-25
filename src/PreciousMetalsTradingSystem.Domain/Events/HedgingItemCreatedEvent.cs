using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Event raised when a new HedgingItem is created
    /// </summary>
    public record HedgingItemCreatedEvent(
        HedgingItemId HedgingItemId,
        HedgingAccountId HedgingAccountId) : DomainEvent(nameof(HedgingItem))
    {
        /// <summary>
        /// Creates a HedgingItemCreatedEvent from a HedgingItem entity
        /// </summary>
        /// <param name="hedgingItem">The hedging item that was created</param>
        /// <returns>A new HedgingItemCreatedEvent</returns>
        public static HedgingItemCreatedEvent FromEntity(HedgingItem hedgingItem)
        {
            return new HedgingItemCreatedEvent(
                hedgingItem.Id,
                hedgingItem.HedgingAccountId
            );
        }
    }
}