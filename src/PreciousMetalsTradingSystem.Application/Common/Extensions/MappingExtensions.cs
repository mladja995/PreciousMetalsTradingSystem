using PreciousMetalsTradingSystem.Application.Trading.Execution.Models;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Common.Extensions
{
    public static class MappingExtensions
    {
        public static string ToAMarkOrderType(this SideType sideType) =>
            sideType switch
            {
                SideType.Buy => Constants.AMARK_ORDER_TYPE_BUY,
                SideType.Sell => Constants.AMARK_ORDER_TYPE_SELL,
                _ => throw new ArgumentOutOfRangeException(nameof(sideType), $"Unknown SideType: {sideType}")
            };

        public static string ToAMarkSpotDeferredProductCode(this MetalType metalType) =>
            metalType switch
            {
                MetalType.XAG => Constants.AMARK_SPOT_DEFERRED_TRADE_PRODUCT_CODE_SILVER,
                MetalType.XAU => Constants.AMARK_SPOT_DEFERRED_TRADE_PRODUCT_CODE_GOLD,
                _ => throw new ArgumentOutOfRangeException(nameof(metalType), $"Unknown MetalType: {metalType}")
            };

        public static MetalType ToMetalTypeFromAMarkSpotDeferredProductCode(this string productCode) =>
            productCode switch
            {
                Constants.AMARK_SPOT_DEFERRED_TRADE_PRODUCT_CODE_SILVER => MetalType.XAG,
                Constants.AMARK_SPOT_DEFERRED_TRADE_PRODUCT_CODE_GOLD => MetalType.XAU,
                _ => throw new ArgumentOutOfRangeException(nameof(productCode), $"Unknown Product Code: {productCode}")
            };

        public static TransactionSideType ToTransactionSideType(this SideType sideType) =>
            sideType switch
            {
                SideType.Buy => TransactionSideType.Debit,
                SideType.Sell => TransactionSideType.Credit,
                _ => throw new ArgumentOutOfRangeException(nameof(sideType), $"Unknown SideType: {sideType}")
            };

        public static PositionSideType ToPositionSideType(this SideType sideType) =>
            sideType switch
            {
                SideType.Buy => PositionSideType.In,
                SideType.Sell => PositionSideType.Out,
                _ => throw new ArgumentOutOfRangeException(nameof(sideType), $"Unknown SideType: {sideType}")
            };

        public static SideType ToOppositeSide(this SideType side) =>
            side == SideType.Buy ? SideType.Sell : SideType.Buy;

        public static PositionSideType ToOppositeSide(this PositionSideType side) => 
            side == PositionSideType.In ? PositionSideType.Out : PositionSideType.In;

        public static TransactionSideType ToOppositeSide(this TransactionSideType side) => 
            side == TransactionSideType.Credit ? TransactionSideType.Debit : TransactionSideType.Credit;

        public static ActivityType ToActivityType(this TradeType tradeType) =>
            tradeType switch
            {
                TradeType.ClientTrade => ActivityType.ClientTrade,
                TradeType.DealerTrade => ActivityType.DealerTrade,
                TradeType.OffsetTrade => ActivityType.OffsetTrade,
                _ => throw new ArgumentOutOfRangeException(nameof(tradeType), $"Unknown Trade Type: {tradeType}")
            };

        public static ClientSideType ToClientSideType(this SideType sideType) =>
           sideType switch
           {
               SideType.Buy => ClientSideType.Sell,
               SideType.Sell => ClientSideType.Buy,
               _ => throw new ArgumentOutOfRangeException(nameof(sideType), $"Unknown Side Type: {sideType}")
           };

        public static SideType ToSideType(this ClientSideType clientSideType) =>
           clientSideType switch
           {
               ClientSideType.Sell => SideType.Buy,
               ClientSideType.Buy => SideType.Sell,
               _ => throw new ArgumentOutOfRangeException(nameof(clientSideType), $"Unknown Client Side Type: {clientSideType}")
           };

        public static string ToEnumOrderValue(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            return  fieldInfo?.Name ??  value.ToString();
        }

        public static Quote ToQuote(this TradeQuote tradeQuote)
        {
            return new Quote
            {
                Id = tradeQuote.Id,
                IssuedAtUtc = tradeQuote.IssuedAtUtc,
                ExpiriesAtUtc = tradeQuote.ExpiresAtUtc,
                Location = tradeQuote.LocationType,
                Side = tradeQuote.Side.ToClientSideType(),
                Items = tradeQuote.Items.Select(x => new QuoteItem
                {
                    ProductSKU = x.Product.ThrowIfNull(() => new Exception($"Product with ID {x.ProductId} not loaded")).Value.SKU,
                    QuantityUnits = x.QuantityUnits,
                    PricePerOz = x.EffectivePricePerOz,
                    TotalAmount = x.CalculateTotalAmount(x.Product.WeightInOz)
                })
            };
        }
    }
}
