using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Models.Notifications
{
    /// <summary>
    /// Notification for when a hedgingItem is created or updated or deleted
    /// </summary>
    public record HedgingItemChangedNotification(
        string HedgingAccountId,
        string HedgingItemId,
        ChangeType ChangeType,
        DateOnly Date,
        HedgingItemType HedgingItemType,
        HedgingItemSideType HedgingItemSideType,
        decimal Amount,
        string Note,
        decimal UnrealizedGainOrLossValue
        )
    {
    }
}
