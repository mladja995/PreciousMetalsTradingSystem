using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Financials.Models.Notifications
{
    /// <summary>
    /// Notification for when a financial balance changes
    /// </summary>
    public record FinancialBalanceChangedNotification(
        string TransactionId,
        DateTime TimestampUtc,
        BalanceType BalanceType,
        TransactionSideType SideType,
        ActivityType ActivityType,
        decimal Amount,
        decimal Balance,
        string Reference,
        bool IsFinancialSettled,
        string Note);
}
