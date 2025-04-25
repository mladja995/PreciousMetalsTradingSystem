using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class SpotDeferredTradeItem : AggregateRoot<SpotDeferredTradeItemId>
    {
        public SpotDeferredTradeId SpotDeferredTradeId { get; private set; }
        public MetalType Metal { get; private set; }
        public Money PricePerOz { get; private set; } 
        public QuantityOunces QuantityOz { get; private set; }
        public Money TotalAmount { get; private set; }

        public virtual SpotDeferredTrade SpotDeferredTrade { get; private set; }


        public static SpotDeferredTradeItem Create(
            MetalType metalType,
            Money pricePerOz,
            QuantityOunces quantityOz)
        {
            pricePerOz.Value.Throw(() => new NegativeOrZeroAmountException()).IfLessThan(0m);

            return new SpotDeferredTradeItem
            {
                Id = SpotDeferredTradeItemId.New(),
                Metal = metalType,
                PricePerOz = pricePerOz,
                QuantityOz = quantityOz,
                TotalAmount = new Money(pricePerOz * quantityOz)
            };
        }

        /// <summary>
        /// Calculates the adjusted total amount for the Spot Deferred Trade Item based on the specified side (Buy or Sell).
        /// If the side is Buy, the amount is positive; if the side is Sell, the amount is negative.
        /// </summary>
        /// <param name="side">The side of the Spot Deferred Trade Item, either Buy or Sell, which determines the adjustment direction.</param>
        /// <returns>The adjusted total amount as a positive or negative decimal based on the Spot Deferred Trade Item's side.</returns>
        public decimal GetAdjustedAmount(SideType side)
        {
            return (int)side * TotalAmount.Value;
        }

        public SpotDeferredTradeItem SetTrade(SpotDeferredTrade trade)
        {
            trade.ThrowIfNull();
            SpotDeferredTrade = trade;
            return this;
        }

        public void SetTrade(SpotDeferredTradeId id) 
            => SpotDeferredTradeId = id;
    }
}
