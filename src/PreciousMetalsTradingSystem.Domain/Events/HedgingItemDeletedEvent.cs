using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Event raised when a new HedgingItem is deleted
    /// </summary>
    public record HedgingItemDeletedEvent(
        HedgingItemId HedgingItemId,
        HedgingAccountId HedgingAccountId,
        DateOnly HedgingItemDate,
        HedgingItemType Type,
        HedgingItemSideType SideType,
        decimal Amount,
        string Note
        ) : DomainEvent(nameof(HedgingItem))
    {
        /// <summary>
        /// Creates a HedgingItemDeletedEvent from a HedgingItem entity
        /// </summary>
        /// <param name="hedgingItem">The HedgingItem that was deleted</param>
        /// <returns>A new HedgingItemDeletedEvent</returns>
        public static HedgingItemDeletedEvent FromEntity(HedgingItem hedgingItem)
        {
            return new HedgingItemDeletedEvent(
            hedgingItem.Id,
            hedgingItem.HedgingAccountId,
            hedgingItem.HedgingItemDate,
            hedgingItem.Type,
            hedgingItem.SideType,
            hedgingItem.Amount,
            hedgingItem.Note ?? string.Empty);
        }
    }
}
