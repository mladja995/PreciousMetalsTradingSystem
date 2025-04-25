using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class SpotDeferredTrade : AggregateRoot<SpotDeferredTradeId>
    {
        public HedgingAccountId HedgingAccountId {  get; private set; }
        public string TradeConfirmationReference { get; private set; }
        public SideType Side {  get; private set; }
        public DateOnly SpotDeferredTradeDate {  get; private set; }
        public DateTime TimestampUtc { get; private set; }
        public bool IsManual { get; private set; }

        public virtual ICollection<SpotDeferredTradeItem> Items { get; } = [];
        public virtual ICollection<Trade> Trades { get; } = [];

        public static SpotDeferredTrade Create(
            HedgingAccountId hedgingAccountId,
            string tradeConfirmationReference,
            SideType sideType,
            DateOnly Date,
            bool isManual = false,
            IEnumerable<SpotDeferredTradeItem>? items = null)
        {
            hedgingAccountId.ThrowIfNull();

            var entity = new SpotDeferredTrade
            {
                Id = SpotDeferredTradeId.New(),
                HedgingAccountId = hedgingAccountId,
                TradeConfirmationReference = tradeConfirmationReference,
                Side = sideType,
                SpotDeferredTradeDate = Date,
                TimestampUtc = DateTime.UtcNow,
                IsManual = isManual
            };

            items?.ToList().ForEach(entity.AddItem);

            // Add domain event
            entity.AddDomainEvent(SpotDeferredTradeCreatedEvent.FromEntity(entity));

            return entity;
        }

        public void AddItem(SpotDeferredTradeItem item)
        {
            EnsureOnlyOneItemPerMetalType(item.Metal);
            Items.Add(item);
        }

        private void EnsureOnlyOneItemPerMetalType(MetalType metal)
        {
            if (Items.Any(lc => lc.Metal == metal))
            {
                throw new DuplicatedSpotDeferredTradeItemPerMetalTypeException(metal);
            }
        }

        public Dictionary<MetalType, Money> GetPricesPerOzByMetalType()
        {
            return Items
                .ToDictionary(x => x.Metal, y => y.PricePerOz);
        }
    }
}
