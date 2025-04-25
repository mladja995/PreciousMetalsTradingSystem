using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using static PreciousMetalsTradingSystem.Domain.Events.ProductCreatedEvent;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Event raised when a new Product is created
    /// </summary>
    public record ProductCreatedEvent(
        ProductId ProductId,
        string Name,
        SKU SKU,
        Weight WeightInOz,
        MetalType MetalType,
        bool IsAvailable,
        IReadOnlyCollection<LocationConfigurationData> LocationConfigurations) : DomainEvent(nameof(Product))
    {
        /// <summary>
        /// Data structure representing a product location configuration
        /// </summary>
        public record LocationConfigurationData(
            LocationType LocationType,
            PremiumUnitType PremiumUnitType,
            Premium BuyPremium,
            Premium SellPremium,
            bool IsAvailableForBuy,
            bool IsAvailableForSell);

        /// <summary>
        /// Creates a ProductCreatedEvent from a Product entity
        /// </summary>
        /// <param name="product">The product that was created</param>
        /// <returns>A new ProductCreatedEvent</returns>
        public static ProductCreatedEvent FromEntity(Product product)
        {
            var configData = product.LocationConfigurations
                .Select(lc => new LocationConfigurationData(
                    lc.LocationType,
                    lc.PremiumUnitType,
                    lc.BuyPremium,
                    lc.SellPremium,
                    lc.IsAvailableForBuy,
                    lc.IsAvailableForSell))
                .ToList();

            return new ProductCreatedEvent(
                product.Id,
                product.Name,
                product.SKU,
                product.WeightInOz,
                product.MetalType,
                product.IsAvailable,
                configData);
        }
    }
}
