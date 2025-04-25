using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Models.Notifications
{
    /// <summary>
    /// Notification for when a product is created or updated
    /// </summary>
    public record ProductCatalogChangedNotification(
        string ProductId,
        ChangeType ChangeType,
        string Name,
        string SKU,
        decimal WeightInOz,
        MetalType MetalType,
        bool IsAvailable,
        IEnumerable<LocationConfigurationData> LocationConfigurations);

    /// <summary>
    /// Data structure representing a product location configuration for the notification
    /// </summary>
    public record LocationConfigurationData(
        string LocationType,
        string PremiumUnitType,
        decimal BuyPremium,
        decimal SellPremium,
        bool IsAvailableForBuy,
        bool IsAvailableForSell);
}
