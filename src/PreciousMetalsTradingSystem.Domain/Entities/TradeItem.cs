using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class TradeItem : Entity<TradeItemId>
    {
        public TradeId TradeId { get; private set; }
        public TradeQuoteItemId? TradeQuoteItemId { get; private set; }
        public ProductId ProductId { get; private set; }
        public QuantityUnits QuantityUnits { get; private set; }
        public Money SpotPricePerOz { get; private set; } // Spot price
        public Money TradePricePerOz { get; private set; } // Physical Spot Price in sheet (Spot price for Client Trades OR Dealer price for Dealer Trades)
        public Premium PremiumPerOz { get; private set; } // From Product Location Configuration
        public Money EffectivePricePerOz { get; private set; } // TradePricePerOz + PremiumPerOz
        public Revenue TotalRevenue { get; private set; } // QuantityUnits * Product.WeightInOz * (EffectivePricePerOz - SpotPricePerOz) * (-Trade.SideType)
        public Money TotalEffectivePrice { get; private set; } // QuantityUnits * Product.WeightInOz * EffectivePricePerOz 

        public virtual Product Product { get; private set; }
        public virtual Trade Trade { get; private set; }

        public static TradeItem Create(
            SideType tradeSide,
            ProductId productId,
            Weight productWeightInOz,
            QuantityUnits quantityUnits,
            Money spotPricePerOz,
            Money tradePricePerOz,
            Premium premiumPerOz,
            Money effectivePricePerOz)
        {
            return new TradeItem
            {
                Id = TradeItemId.New(),
                ProductId = productId,
                QuantityUnits = quantityUnits,
                SpotPricePerOz = spotPricePerOz,
                TradePricePerOz = tradePricePerOz,
                PremiumPerOz = premiumPerOz,
                EffectivePricePerOz = effectivePricePerOz,
                TotalRevenue = CalculateRevenue(
                    tradeSide,
                    quantityUnits,
                    productWeightInOz,
                    effectivePricePerOz.Value - spotPricePerOz.Value),
                TotalEffectivePrice = CalculateTotalEffectivePricePerOz(
                    quantityUnits,
                    productWeightInOz,
                    effectivePricePerOz)
            };
        }

        public void SetTradeQuoteItem(TradeQuoteItem tradeQuoteItem)
        {
            tradeQuoteItem.ThrowIfNull();
            TradeQuoteItemId = tradeQuoteItem.Id;
        }

        public TradeItem SetProduct(Product product)
        {
            product.ThrowIfNull();
            Product = product;
            return this;
        }

        internal TradeItem SetRevenue(Revenue value)
        {
            value.ThrowIfNull();
            TotalRevenue = value;

            return this;
        }

        private static Revenue CalculateRevenue(
            SideType tradeSide,
            QuantityUnits quantity,
            Weight productWeightInOz,
            decimal spread)
            => new(-(int)tradeSide * quantity * productWeightInOz * spread);

        private static Money CalculateTotalEffectivePricePerOz(
            QuantityUnits quantity,
            Weight productWeightInOz,
            Money pricePerOz)
            => new(quantity * productWeightInOz * pricePerOz);
    }
}
