using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class HedgingAccount : AggregateRoot<HedgingAccountId>
    {
        public HedgingAccountName Name { get; private set; }  
        public HedgingAccountCode Code { get; private set; }


        public virtual ICollection<SpotDeferredTrade> SpotDeferredTrades { get; } = [];
        public virtual ICollection<HedgingItem> HedgingItems { get; } = [];
        public virtual ICollection<LocationHedgingAccountConfiguration> LocationHedgingAccountConfigurations { get; } = [];


        public static HedgingAccount Create(
            HedgingAccountName name, 
            HedgingAccountCode code)
        {
            return new HedgingAccount
            {
                Id = HedgingAccountId.New(),
                Name = name,
                Code = code
            };
        }

        public void AddHedgingItem(HedgingItem item)
            => HedgingItems.Add(item);

        public void AddSpotDeferredTrade(SpotDeferredTrade trade)
            => SpotDeferredTrades.Add(trade);


        /// <summary>
        /// Calculates the unrealized gain or loss value for the hedging account.
        /// Requires that SpotDeferredTrades and HedgingItems collections are populated,
        /// including nested items within SpotDeferredTrades.
        /// </summary>
        /// <returns>The calculated unrealized gain or loss value.</returns>
        public decimal CalculateUnrealizedGainOrLossValue()
        {
            var spotDeferredTradesNetAmount =
                SpotDeferredTrades
                    .SelectMany(x => x.Items, (x, y) => new { Item = y, SpotDeferredTrade = x })
                    .Sum(z => z.Item.GetAdjustedAmount(z.SpotDeferredTrade.Side));

            var profitLossesNetAmount =
                HedgingItems
                    .Where(x => x.Type.Equals(HedgingItemType.ProfitLosses))
                    .Sum(x => x.GetAdjustedAmount());

            var monthlyFeeNetAmount =
                HedgingItems
                    .Where(x => x.Type.Equals(HedgingItemType.MonthlyFee))
                    .Sum(x => x.GetAdjustedAmount());

            return spotDeferredTradesNetAmount + profitLossesNetAmount - monthlyFeeNetAmount;
        }
    }
}
