using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models.Notifications
{
    /// <summary>
    /// Notification for when a spot deferred trade is created
    /// </summary>
    public record SpotDeferredTradeCreatedNotification(
        string HedgingAccountId,
        decimal UnrealizedGainOrLossValue,
        IEnumerable<SpotDeferredTradeItemData> Items);

    /// <summary>
    /// Data structure representing a spot deferred trade item for the notification
    /// </summary>
    public record SpotDeferredTradeItemData(
        string TradeConfirmationReference,
        string? TradeReference,
        TradeType? TradeType,
        DateTime TimestampUtc,
        SideType SideType,
        MetalType MetalType,
        decimal PricePerOz,
        decimal QuantityOz,
        decimal TotalAmount,
        decimal SummaryActualTradedBalance, 
        decimal SummaryNetAmount,
        DateOnly SummaryLastHedgingDate);
}
