using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using static PreciousMetalsTradingSystem.Domain.Events.ProductUpdatedEvent;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    public record ProductUpdatedEvent(
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
        /// Creates a ProductUpdatedEvent from a Product entity
        /// </summary>
        /// <param name="product">The product that was updated</param>
        /// <returns>A new ProductUpdatedEvent</returns>
        public static ProductUpdatedEvent FromEntity(Product product)
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

            return new ProductUpdatedEvent(
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
