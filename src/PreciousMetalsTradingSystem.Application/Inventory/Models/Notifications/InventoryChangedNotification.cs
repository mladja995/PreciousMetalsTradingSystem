using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Inventory.Models.Notifications
{
    /// <summary>
    /// Notification for when inventory changes occur
    /// </summary>
    public record InventoryChangedNotification(
        string PositionId,
        string ProductId,
        string ProductName,
        string SKU,
        LocationType LocationType,
        PositionSideType SideType,
        PositionType PositionType,
        int QuantityUnits,
        int PositionUnits,
        DateTime TimestampUtc,
        string Reference,
        TradeType TradeType);
}
