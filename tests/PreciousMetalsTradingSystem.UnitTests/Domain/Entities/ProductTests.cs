using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Tests.Domain.Entities
{
    public class ProductTests
    {
        #region CalculatePricePerOz Tests

        [Fact]
        public void CalculatePricePerOz_ShouldReturnCorrectPrice_WhenPremiumUnitTypeIsDollars()
        {
            // Arrange
            var product = Product.Create("Gold Bar", new SKU("SKU001"), new Weight(10), MetalType.XAU, true);
            var configuration = ProductLocationConfiguration.Create(
                LocationType.SLC,
                PremiumUnitType.Dollars,
                new Premium(5),
                new Premium(3),
                true,
                true);
            product.AddLocationConfiguration(configuration);

            var spotPrice = new Money(100);

            // Act
            var result = product.CalculatePricePerOz(spotPrice, LocationType.SLC, SideType.Buy);

            // Assert
            Assert.Equal(new Money(105), result);
        }

        [Fact]
        public void CalculatePricePerOz_ShouldReturnCorrectPrice_WhenPremiumUnitTypeIsPercentage()
        {
            // Arrange
            var product = Product.Create("Gold Bar", new SKU("SKU001"), new Weight(10), MetalType.XAU, true);
            var configuration = ProductLocationConfiguration.Create(
                LocationType.SLC,
                PremiumUnitType.Percentage,
                new Premium(10),
                new Premium(5),
                true,
                true);
            product.AddLocationConfiguration(configuration);

            var spotPrice = new Money(100);

            // Act
            var result = product.CalculatePricePerOz(spotPrice, LocationType.SLC, SideType.Buy);

            // Assert
            Assert.Equal(new Money(110), result);
        }

        [Fact]
        public void CalculatePricePerOz_ShouldThrowPremiumNotDefinedException_WhenPremiumIsNull()
        {
            // Arrange
            var product = Product.Create("Silver Bar", new SKU("SKU002"), new Weight(5), MetalType.XAU, true);

            // Act & Assert
            Assert.Throws<PremiumNotDefinedException>(() =>
                product.CalculatePricePerOz(new Money(100), LocationType.SLC, SideType.Buy));
        }

        [Fact]
        public void CalculatePricePerOz_ShouldThrowUnsupportedPremiumUnitTypeException_WhenPremiumUnitTypeIsUnsupported()
        {
            // Arrange
            var product = Product.Create("Gold Bar", new SKU("SKU004"), new Weight(15), MetalType.XAU, true);
            var configuration = ProductLocationConfiguration.Create(
                LocationType.SLC,
                (PremiumUnitType)999, // Invalid enum value
                new Premium(10),
                new Premium(5),
                true,
                true);
            product.AddLocationConfiguration(configuration);

            // Act & Assert
            Assert.Throws<UnsupportedPremiumUnitTypeException>(() =>
                product.CalculatePricePerOz(new Money(100), LocationType.SLC, SideType.Buy));
        }

        #endregion

        #region IsAvailableForTrading Tests

        [Fact]
        public void IsAvailableForTrading_ShouldReturnFalse_WhenProductIsUnavailableGlobally()
        {
            // Arrange
            var product = Product.Create("Silver Bar", new SKU("SKU005"), new Weight(10), MetalType.XAU, false);

            // Act
            var result = product.IsAvailableForTrading(LocationType.SLC, SideType.Buy);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsAvailableForTrading_ShouldReturnFalse_WhenNoConfigurationExistsForLocation()
        {
            // Arrange
            var product = Product.Create("Gold Bar", new SKU("SKU006"), new Weight(10), MetalType.XAU, true);

            // Act
            var result = product.IsAvailableForTrading(LocationType.SLC, SideType.Buy);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsAvailableForTrading_ShouldReturnCorrectValue_WhenConfigurationExists()
        {
            // Arrange
            var product = Product.Create("Gold Bar", new SKU("SKU007"), new Weight(10), MetalType.XAU, true);
            var configuration = ProductLocationConfiguration.Create(
                LocationType.SLC,
                PremiumUnitType.Dollars,
                new Premium(5),
                new Premium(3),
                true,
                false);
            product.AddLocationConfiguration(configuration);

            // Act
            var buyResult = product.IsAvailableForTrading(LocationType.SLC, SideType.Buy);
            var sellResult = product.IsAvailableForTrading(LocationType.SLC, SideType.Sell);

            // Assert
            Assert.True(buyResult);
            Assert.False(sellResult);
        }

        #endregion

        #region AddLocationConfiguration Tests

        [Fact]
        public void AddLocationConfiguration_ShouldAllowAddingDifferentLocations()
        {
            // Arrange
            var product = Product.Create("Gold Bar", new SKU("SKU008"), new Weight(10), MetalType.XAU, true);
            var config1 = ProductLocationConfiguration.Create(
                LocationType.SLC,
                PremiumUnitType.Dollars,
                new Premium(5),
                new Premium(3),
                true,
                true);
            var config2 = ProductLocationConfiguration.Create(
                LocationType.IDS_DE,
                PremiumUnitType.Percentage,
                new Premium(10),
                new Premium(5),
                true,
                true);

            // Act
            product.AddLocationConfiguration(config1);
            product.AddLocationConfiguration(config2);

            // Assert
            Assert.Equal(2, product.LocationConfigurations.Count);
        }

        [Fact]
        public void AddLocationConfiguration_ShouldThrowException_WhenDuplicateLocationIsAdded()
        {
            // Arrange
            var product = Product.Create("Gold Bar", new SKU("SKU009"), new Weight(10), MetalType.XAU, true);
            var config1 = ProductLocationConfiguration.Create(
                LocationType.SLC,
                PremiumUnitType.Dollars,
                new Premium(5),
                new Premium(3),
                true,
                true);

            var config2 = ProductLocationConfiguration.Create(
                LocationType.SLC,
                PremiumUnitType.Percentage,
                new Premium(10),
                new Premium(5),
                true,
                true);

            product.AddLocationConfiguration(config1);

            // Act & Assert
            Assert.Throws<DuplicatedProductLocationConfigurationException>(() =>
                product.AddLocationConfiguration(config2));
        }

        #endregion
    }
}
