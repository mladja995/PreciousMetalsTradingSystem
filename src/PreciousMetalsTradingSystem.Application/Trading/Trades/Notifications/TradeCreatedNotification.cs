using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Notifications
{
    /// <summary>
    /// Notification for when a trade is created
    /// </summary>
    public record TradeCreatedNotification(
        string TradeId,
        string TradeNumber,
        TradeType TradeType,
        SideType SideType,
        LocationType LocationType,
        DateOnly TradeDate,
        DateTime TimestampUtc,
        string Note,
        bool IsPositionSettled,
        DateTime? PositionSettledOnUtc,
        bool IsFinancialSettled,
        DateTime? FinancialSettledOnUtc,
        DateTime? ConfirmedOnUtc,
        IEnumerable<TradeItemData> Items);

    /// <summary>
    /// Data structure representing a trade item for the notification
    /// </summary>
    public record TradeItemData(
        string TradeItemId,
        string ProductId,
        string ProductName,
        string ProductSKU,
        int QuantityUnits,
        decimal SpotPricePerOz,
        decimal TradePricePerOz,
        decimal PremiumPerOz,
        decimal EffectivePricePerOz,
        decimal TotalRevenue,
        decimal TotalEffectivePrice);
}
