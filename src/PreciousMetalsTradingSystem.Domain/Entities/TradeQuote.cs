using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class TradeQuote : AggregateRoot<TradeQuoteId>
    {
        public string DealerQuoteId { get; private set; }
        public DateTime IssuedAtUtc { get; private set; }
        public DateTime ExpiresAtUtc { get; private set; }
        public TradeQuoteStatusType Status { get; private set; }
        public SideType Side { get; private set; }
        public LocationType LocationType { get; private set; }
        public string? Note { get; private set; }
        public virtual ICollection<TradeQuoteItem> Items { get; } = [];

        public static TradeQuote Create(
            string dealerQuoteId,
            DateTime issuedAtUtc,
            DateTime expiresAtUtc,
            SideType side,
            LocationType locationType,
            string? note = null)
        {
            return new TradeQuote
            {
                Id = TradeQuoteId.New(),
                DealerQuoteId = dealerQuoteId,
                IssuedAtUtc = issuedAtUtc,
                ExpiresAtUtc = expiresAtUtc,
                Status = TradeQuoteStatusType.Pending,
                Side = side,
                LocationType = locationType,
                Note = note
            };
        }

        public void AddItem(TradeQuoteItem item)
        {
            Items.Add(item);
        }

        public void MarkAsExecuted()
        {
            if (Status != TradeQuoteStatusType.Pending)
            {
                throw new InvalidOperationException("Invalid Trade Quote state");
            }

            SetStatus(TradeQuoteStatusType.Executed);
        }

        public void MarkAsExpired()
        {
            if (Status != TradeQuoteStatusType.Pending)
            {
                throw new InvalidOperationException("Invalid Trade Quote state");
            }

            SetStatus(TradeQuoteStatusType.Expired);
        }

        private void SetStatus(TradeQuoteStatusType statusType)
            => Status = statusType;

        public Dictionary<Product, QuantityUnits> GetQuantityPerProduct()
        {
            return Items
                .GroupBy(x => x.Product)
                .Select(x => new { Product = x.Key, Quantity = x.Sum(y => y.QuantityUnits) })
                .ToDictionary(x => x.Product, y => new QuantityUnits(y.Quantity));
        }

        public Dictionary<MetalType, QuantityOunces> GetQuantityPerMetalType()
        {
            return Items
                .GroupBy(x => x.Product.MetalType)
                .Select(x => new { MetalType = x.Key, Quantity = x.Sum(y => y.Product.WeightInOz * y.QuantityUnits) })
                .ToDictionary(x => x.MetalType, y => new QuantityOunces(y.Quantity));
        }

        public Dictionary<MetalType, Money> GetSpotPricesPerMetalType()
        {
            return Items
                .GroupBy(x => x.Product.MetalType)
                .Select(x => new { MetalType = x.Key, x.First().SpotPricePerOz })
                .ToDictionary(x => x.MetalType, y => y.SpotPricePerOz);
        }
    }
}
