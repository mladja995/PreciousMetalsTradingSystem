using PreciousMetalsTradingSystem.Application.Trading.Trades.Notifications.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Notifications
{
    public record TradeStateChangedNotification(
        string Id, 
        string TradeNumber, 
        TradeStateChangeType ChangeType, 
        DateTime TimestampUtc);
}
