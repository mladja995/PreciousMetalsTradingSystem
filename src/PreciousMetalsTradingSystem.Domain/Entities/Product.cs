using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class Product : AggregateRoot<ProductId>
    {
        public string Name { get; private set; }
        public SKU SKU { get; private set; }
        public Weight WeightInOz { get; private set; }
        public MetalType MetalType { get; private set; }
        public bool IsAvailable { get; private set; }

        public virtual ICollection<ProductLocationConfiguration> LocationConfigurations { get; } = [];

        public static Product Create(
            string name,
            SKU sku,
            Weight weightInOz,
            MetalType metalType,
            bool isAvailable,
            IEnumerable<ProductLocationConfiguration>? configurations = null)
        {
            var entity = new Product
            {
                Id = ProductId.New(),
                Name = name,
                SKU = sku,
                WeightInOz = weightInOz,
                MetalType = metalType,
                IsAvailable = isAvailable
            };

            configurations?.ToList().ForEach(entity.AddLocationConfiguration);

            entity.AddDomainEvent(ProductCreatedEvent.FromEntity(entity));

            return entity;
        }

        public void UpdateProductDetails(
            string name,
            SKU sku,
            Weight weightInOz,
            MetalType metalType,
            bool isAvailable,
            IEnumerable<ProductLocationConfiguration> newConfigurations)
        {
            Name = name;
            SKU = sku;
            WeightInOz = weightInOz;
            MetalType = metalType;
            IsAvailable = isAvailable;

            UpdateLocationConfigurations(newConfigurations);
            AddDomainEvent(ProductUpdatedEvent.FromEntity(this));
        }

        public void AddLocationConfiguration(ProductLocationConfiguration locationConfiguration)
        {
            EnsureOnlyOneConfigurationPerLocation(locationConfiguration.LocationType);
            LocationConfigurations.Add(locationConfiguration);
        }

        public bool IsAvailableForTrading(LocationType location, SideType sideType)
        {
            if (!IsAvailable) return false;

            var configuration = LocationConfigurations
                .SingleOrDefault(lc => lc.LocationType == location);

            if (configuration == null) return false;

            return sideType switch
            {
                SideType.Buy => configuration.IsAvailableForBuy,
                SideType.Sell => configuration.IsAvailableForSell,
                _ => false
            };
        }

        public PremiumUnitType? GetPremiumUnitType(LocationType location)
            => LocationConfigurations.SingleOrDefault(lc => lc.LocationType == location)?.PremiumUnitType;

        public Premium? GetPremium(LocationType location, SideType side)
        {
            var configuration = LocationConfigurations
                .SingleOrDefault(lc => lc.LocationType == location);
            
            if (configuration == null) return null;

            return side switch
            {
                SideType.Buy => configuration.BuyPremium,
                SideType.Sell => configuration.SellPremium,
                _ => null
            };
        }

        public Money CalculatePricePerOz(
            Money spotPricePerOz,
            LocationType location,
            SideType side)
        {
            var premium = GetPremium(location, side);
            premium.ThrowIfNull(() => new PremiumNotDefinedException(location, side));

            var premiumUnitType = GetPremiumUnitType(location);
            premiumUnitType.ThrowIfNull(() => new PremiumUnitTypeNotDefinedException(location));
            
            var finalPricePerOz = premiumUnitType switch
            {
                PremiumUnitType.Dollars => spotPricePerOz + premium.Value,
                PremiumUnitType.Percentage => spotPricePerOz * (1 + (premium.Value / 100)),
                _ => throw new UnsupportedPremiumUnitTypeException(premiumUnitType!.Value)
            };
            return new Money(finalPricePerOz);
        }

        #region Private

        private void UpdateLocationConfigurations(IEnumerable<ProductLocationConfiguration> newConfigurations)
        {
            LocationConfigurations.Clear(); 
            foreach (var configuration in newConfigurations)
            {
                AddLocationConfiguration(configuration); 
            }
        }

        private void EnsureOnlyOneConfigurationPerLocation(LocationType location)
        {
            if (LocationConfigurations.Any(lc => lc.LocationType == location))
            {
                throw new DuplicatedProductLocationConfigurationException(location);
            }
        }

        #endregion
    }
}
