using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class TradeQuoteItem : Entity<TradeQuoteItemId>
    {
        public TradeQuoteId TradeQuoteId { get; private set; }
        public ProductId ProductId { get; private set; }
        public QuantityUnits QuantityUnits { get; private set; }
        public Money SpotPricePerOz { get; private set; }
        public Premium PremiumPricePerOz { get; private set; }
        public Money EffectivePricePerOz { get; private set; }

        public virtual Product Product { get; private set; }
        public virtual TradeQuote TradeQuote { get; private set; }

        public static TradeQuoteItem Create(
            ProductId productId,
            QuantityUnits quantityUnits,
            Money spotPricePerOz,
            Premium premiumPricePerOz,
            Money effectivePricePerOz)
        {
            return new TradeQuoteItem
            {
                Id = TradeQuoteItemId.New(),
                ProductId = productId,
                QuantityUnits = quantityUnits,
                SpotPricePerOz = spotPricePerOz,
                PremiumPricePerOz = premiumPricePerOz,
                EffectivePricePerOz = effectivePricePerOz,
            };
        }

        public static TradeQuoteItem Create(
            Product product,
            QuantityUnits quantityUnits,
            Money spotPricePerOz,
            Premium premiumPricePerOz,
            Money effectivePricePerOz)
        {
            var item = Create(product.Id, quantityUnits, spotPricePerOz, premiumPricePerOz, effectivePricePerOz);
            item.Product = product;
            return item;
        }

        public Money CalculateTotalAmount(Weight productWeightInOz)
            => new(QuantityUnits * productWeightInOz * EffectivePricePerOz);
    }
}
