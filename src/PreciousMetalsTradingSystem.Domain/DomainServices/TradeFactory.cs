using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.DomainServices
{
    public class TradeFactory : ITradeFactory
    {
        public Trade CreateOffsetTrade(Trade originalTrade)
        {
            originalTrade.ThrowIfNull();
            originalTrade.Items.ToList().Throw().IfEmpty();
            originalTrade.Items.ToList().ForEach(item => item.Product.ThrowIfNull());
            
            var currentEstDate = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")));

            // TODO: Consider to change FinancilsSettleOn to be nullable DateTime
            var offsetTrade = Trade.Create(
                TradeType.OffsetTrade,
                GetOppositeSide(originalTrade.Side),
                originalTrade.LocationType,
                currentEstDate,
                currentEstDate,
                $"Offset trade as consequence of trade cancellation with number {originalTrade.TradeNumber}");

            offsetTrade.ConfirmedOnUtc = DateTime.UtcNow;

            originalTrade.Items.ToList()
                .ForEach(x => offsetTrade.AddItem(
                    TradeItem.Create(
                        GetOppositeSide(originalTrade.Side),
                        x.ProductId,
                        x.Product.WeightInOz,
                        x.QuantityUnits,
                        x.SpotPricePerOz,
                        x.TradePricePerOz,
                        x.PremiumPerOz,
                        x.EffectivePricePerOz)
                    .SetRevenue(new Revenue(0m)))); 

            return offsetTrade;
        }

        private static SideType GetOppositeSide(SideType side) =>
            side == SideType.Buy ? SideType.Sell : SideType.Buy;
    }
}
