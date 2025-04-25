using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class HedgingItem : AggregateRoot<HedgingItemId>
    {
        public HedgingAccountId HedgingAccountId { get; private set; }
        public DateOnly HedgingItemDate { get; private set; }
        public DateTime TimestampUtc { get; private set; }
        public HedgingItemType Type { get; private set; }
        public HedgingItemSideType SideType { get; private set; }
        public Money Amount { get; private set; }
        public string? Note { get; private set; }

        public static HedgingItem Create(
            HedgingAccountId hedgingAccountId,
            DateOnly date,
            HedgingItemType type,
            HedgingItemSideType sideType,
            Money amount,
            string? note = null)
        {
            hedgingAccountId.ThrowIfNull();

            var entity = new HedgingItem
            {
                Id = HedgingItemId.New(),
                HedgingAccountId = hedgingAccountId,
                HedgingItemDate = date,
                TimestampUtc = DateTime.UtcNow,
                Type = type,
                SideType = sideType,
                Amount = amount,
                Note = note
            };

            entity.AddDomainEvent(HedgingItemCreatedEvent.FromEntity(entity));

            return entity;
        }

        public void Update(
            DateOnly date,
            HedgingItemType type,
            HedgingItemSideType sideType,
            Money amount,
            string? note = null)
        {
            HedgingItemDate = date;
            Type = type;
            SideType = sideType;
            Amount = amount;
            Note = note;

            AddDomainEvent(HedgingItemUpdatedEvent.FromEntity(this));
        }

        /// <summary>
        /// Calculates the adjusted amount for the hedging item based on its side type.
        /// Multiplies the amount value by the SideType factor (1 for WireIn, -1 for WireOut),
        /// adjusting the amount to reflect positive or negative cash flow as appropriate.
        /// </summary>
        /// <returns>The adjusted amount as a decimal, positive for WireIn and negative for WireOut.</returns>
        public decimal GetAdjustedAmount()
        {
            return (int)SideType * Amount.Value;
        }
    }
}
