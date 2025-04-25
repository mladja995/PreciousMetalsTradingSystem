using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Event raised when a new HedgingItem is updated
    /// </summary>
    public record HedgingItemUpdatedEvent(
        HedgingItemId HedgingItemId,
        HedgingAccountId HedgingAccountId) : DomainEvent(nameof(HedgingItem))
    {
        /// <summary>
        /// Creates a HedgingItemUpdatedEvent from a HedgingItem entity
        /// </summary>
        /// <param name="hedgingItem">The HedgingItem that was updated</param>
        /// <returns>A new HedgingItemUpdatedEvent</returns>
        public static HedgingItemUpdatedEvent FromEntity(HedgingItem hedgingItem)
        {
            return new HedgingItemUpdatedEvent(
            hedgingItem.Id,
            hedgingItem.HedgingAccountId);
        }
    }
}
