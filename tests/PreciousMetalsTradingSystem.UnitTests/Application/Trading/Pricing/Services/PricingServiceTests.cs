using PreciousMetalsTradingSystem.Application.Trading.Pricing.Services;
using PreciousMetalsTradingSystem.Application.Trading.Pricing.Models;
using PreciousMetalsTradingSystem.Application.AMark.Models;
using PreciousMetalsTradingSystem.Application.AMark.Services;
using PreciousMetalsTradingSystem.Application.Caching;
using PreciousMetalsTradingSystem.Application.Common.Options;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Services;
using PreciousMetalsTradingSystem.Domain.Enums;
using Microsoft.Extensions.Options;
using Moq;
using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Application.Common.Extensions;

namespace PreciousMetalsTradingSystem.Tests.Application.Trading.Pricing.Services
{
    public class PricingServiceTests
    {
        private readonly Mock<IAMarkTradingServiceFactory> _factoryMock;
        private readonly Mock<IProductsService> _productsServiceMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly IOptions<ApiSettingsOptions> _options;
        private readonly PricingService _pricingService;

        public PricingServiceTests()
        {
            _factoryMock = new Mock<IAMarkTradingServiceFactory>();
            _productsServiceMock = new Mock<IProductsService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _options = Options.Create(new ApiSettingsOptions
            {
                SpotPricesRefreshCacheFrequencyInMinutes = 30
            });

            _pricingService = new PricingService(
                _options,
                _factoryMock.Object,
                _productsServiceMock.Object,
                _cacheServiceMock.Object);
        }

        [Fact]
        public async Task GetSpotPricesAsync_ShouldReturnPrices_WhenCacheIsValid()
        {
            // Arrange
            var location = LocationType.SLC;
            var side = SideType.Buy;

            var product = Product.Create(
                    "Gold Bar",
                    new SKU("SKU001"),
                    new Weight(1),
                    MetalType.XAU,
                    true);

            product.AddLocationConfiguration(
                    ProductLocationConfiguration.Create(
                        location,
                        PremiumUnitType.Dollars,
                        new Premium(10.3m),
                        new Premium(11.7m),
                        true,
                        true));

            var products = new List<Product> { product };

            var cachedSpotPrices = new QuoteResponse
            {
                QuoteKey = "cachedKey",
                QuoteProductsPricingList =
                [
                    new()
                    {
                        ProductCode = product.MetalType.ToAMarkSpotDeferredProductCode(),
                        SpotPrice = 1800.50m
                    }
                ]
            };

            var cachedTime = DateTime.UtcNow;

            _cacheServiceMock
                .Setup(x => x.GetAsync<QuoteResponse>($"raw_spot_prices_items_{location}_{side}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedSpotPrices);

            _cacheServiceMock
                .Setup(x => x.GetAsync<DateTimeWrapper>($"raw_spot_prices_last_cached_time_{location}_{side}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DateTimeWrapper { Value = cachedTime });

            _productsServiceMock
                .Setup(x => x.GetAvailableProducts(location, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            // Act
            var result = await _pricingService.GetSpotPricesAsync(location, side);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            var productPrice = result.First();
            Assert.Equal("SKU001", productPrice.ProductSKU);
            Assert.Equal(1800.50m, productPrice.SpotPricePerOz);

            // Verify that fetching raw spot prices never happened
            _factoryMock.Verify(x => x.CreateAsync(It.IsAny<LocationType>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetSpotPricesAsync_ShouldFetchFromService_WhenCacheIsStale()
        {
            // Arrange
            var location = LocationType.SLC;
            var side = SideType.Buy;

            var product = Product.Create(
                    "Silver Bar",
                    new SKU("SKU002"),
                    new Weight(1),
                    MetalType.XAG,
                    true);

            product.AddLocationConfiguration(
                    ProductLocationConfiguration.Create(
                        location,
                        PremiumUnitType.Dollars,
                        new Premium(1.3m),
                        new Premium(2.01m),
                        true,
                        true));

            var products = new List<Product> { product };

            var staleTime = DateTime.UtcNow.AddMinutes(-31); // Cache is stale
            var freshQuoteResponse = new QuoteResponse
            {
                QuoteKey = "freshKey",
                QuoteProductsPricingList =
                [
                    new()
                    {
                        ProductCode = product.MetalType.ToAMarkSpotDeferredProductCode(),
                        SpotPrice = 24.50m
                    }
                ]
            };

            var amarkServiceMock = new Mock<IAMarkTradingService>();
            amarkServiceMock
                .Setup(x => x.RequestOnlineQuoteAsync(It.IsAny<OnlineQuoteRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(freshQuoteResponse);

            _factoryMock
                .Setup(x => x.CreateAsync(location, It.IsAny<CancellationToken>()))
                .ReturnsAsync(amarkServiceMock.Object);

            _cacheServiceMock
                .Setup(x => x.GetAsync<QuoteResponse>($"raw_spot_prices_items_{location}_{side}", It.IsAny<CancellationToken>()))
                .ReturnsAsync((QuoteResponse?)null);

            _cacheServiceMock
                .Setup(x => x.GetAsync<DateTimeWrapper>($"raw_spot_prices_last_cached_time_{location}_{side}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DateTimeWrapper { Value = staleTime });

            _productsServiceMock
                .Setup(x => x.GetAvailableProducts(location, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            _cacheServiceMock
                .Setup(x => x.SetAsync($"raw_spot_prices_items_{location}_{side}", It.IsAny<QuoteResponse>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _cacheServiceMock
                .Setup(x => x.SetAsync($"raw_spot_prices_last_cached_time_{location}_{side}", It.IsAny<DateTimeWrapper>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _pricingService.GetSpotPricesAsync(location, side);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            var productPrice = result.First();
            Assert.Equal("SKU002", productPrice.ProductSKU);
            Assert.Equal(24.50m, productPrice.SpotPricePerOz);

            // Verify that fetching raw spot prices happened
            _factoryMock.Verify(x => x.CreateAsync(location, It.IsAny<CancellationToken>()), Times.Once);
            amarkServiceMock.Verify(x => x.RequestOnlineQuoteAsync(It.IsAny<OnlineQuoteRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSpotPricesAsync_ShouldHandleEmptyProducts()
        {
            // Arrange
            var location = LocationType.SLC;
            var side = SideType.Buy;

            _productsServiceMock
                .Setup(x => x.GetAvailableProducts(location, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            var amarkServiceMock = new Mock<IAMarkTradingService>();
            amarkServiceMock
                .Setup(x => x.RequestOnlineQuoteAsync(It.IsAny<OnlineQuoteRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new QuoteResponse
                    {
                        QuoteKey = Guid.NewGuid().ToString(),
                        QuoteProductsPricingList = []
                    });

            _factoryMock
               .Setup(x => x.CreateAsync(location, It.IsAny<CancellationToken>()))
               .ReturnsAsync(amarkServiceMock.Object);

            // Act
            var result = await _pricingService.GetSpotPricesAsync(location, side);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSpotPricesAsync_ShouldReturnPricesForAllLocationsAndSides_WhenLocationAndSideAreNull()
        {
            // Arrange
            var allLocations = Enum.GetValues(typeof(LocationType)).Cast<LocationType>().ToList();
            var allSides = Enum.GetValues(typeof(SideType)).Cast<SideType>().ToList();

            var product = Product.Create(
                "Generic Product",
                new SKU("SKU_Generic"),
                new Weight(1),
                MetalType.XAU,
                true);

            // Add configurations for all locations
            foreach (var location in allLocations)
            {
                product.AddLocationConfiguration(
                    ProductLocationConfiguration.Create(
                        location,
                        PremiumUnitType.Dollars,
                        new Premium(10.0m),
                        new Premium(12.0m),
                        true,
                        true));
            }

            var products = new List<Product> { product };

            var cachedSpotPrices = new QuoteResponse
            {
                QuoteKey = "cachedKey",
                QuoteProductsPricingList = products.Select(p => new QuoteProductPricing
                {
                    ProductCode = p.MetalType.ToAMarkSpotDeferredProductCode(),
                    SpotPrice = 1800.50m
                }).ToList()
            };

            var cachedTime = DateTime.UtcNow;

            _cacheServiceMock
                .Setup(x => x.GetAsync<QuoteResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedSpotPrices);

            _cacheServiceMock
                .Setup(x => x.GetAsync<DateTimeWrapper>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DateTimeWrapper { Value = cachedTime });

            _productsServiceMock
                .Setup(x => x.GetAvailableProducts(It.IsAny<LocationType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            // Act
            var result = await _pricingService.GetSpotPricesAsync(null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(allLocations.Count * allSides.Count, result.Count());
        }

        [Fact]
        public async Task GetSpotPricesAsync_ShouldReturnPricesForGivenLocationAndAllSides_WhenSideIsNull()
        {
            // Arrange
            var location = LocationType.SLC;
            var allSides = Enum.GetValues(typeof(SideType)).Cast<SideType>().ToList();

            var product = Product.Create(
                "Generic Product",
                new SKU("SKU_Generic"),
                new Weight(1),
                MetalType.XAU,
                true);

            // Add configurations for all locations
            foreach (var loc in Enum.GetValues(typeof(LocationType)).Cast<LocationType>())
            {
                product.AddLocationConfiguration(
                    ProductLocationConfiguration.Create(
                        loc,
                        PremiumUnitType.Dollars,
                        new Premium(10.0m),
                        new Premium(12.0m),
                        true,
                        true));
            }

            var products = new List<Product> { product };

            var cachedSpotPrices = new QuoteResponse
            {
                QuoteKey = "cachedKey",
                QuoteProductsPricingList = products.Select(p => new QuoteProductPricing
                {
                    ProductCode = p.MetalType.ToAMarkSpotDeferredProductCode(),
                    SpotPrice = 1800.50m
                }).ToList()
            };

            var cachedTime = DateTime.UtcNow;

            _cacheServiceMock
                .Setup(x => x.GetAsync<QuoteResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedSpotPrices);

            _cacheServiceMock
                .Setup(x => x.GetAsync<DateTimeWrapper>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DateTimeWrapper { Value = cachedTime });

            _productsServiceMock
                .Setup(x => x.GetAvailableProducts(location, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            // Act
            var result = await _pricingService.GetSpotPricesAsync(location, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(allSides.Count, result.Count());
        }

        [Fact]
        public async Task GetSpotPricesAsync_ShouldReturnPricesForAllLocationsAndGivenSide_WhenLocationIsNull()
        {
            // Arrange
            var side = SideType.Sell;

            var product = Product.Create(
                "Generic Product",
                new SKU("SKU_Generic"),
                new Weight(1),
                MetalType.XAU,
                true);

            // Add configurations for all locations
            foreach (var loc in Enum.GetValues(typeof(LocationType)).Cast<LocationType>())
            {
                product.AddLocationConfiguration(
                    ProductLocationConfiguration.Create(
                        loc,
                        PremiumUnitType.Dollars,
                        new Premium(10.0m),
                        new Premium(12.0m),
                        true,
                        true));
            }

            var products = new List<Product> { product };

            var cachedSpotPrices = new QuoteResponse
            {
                QuoteKey = "cachedKey",
                QuoteProductsPricingList = products.Select(p => new QuoteProductPricing
                {
                    ProductCode = p.MetalType.ToAMarkSpotDeferredProductCode(),
                    SpotPrice = 1800.50m
                }).ToList()
            };

            var cachedTime = DateTime.UtcNow;

            _cacheServiceMock
                .Setup(x => x.GetAsync<QuoteResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedSpotPrices);

            _cacheServiceMock
                .Setup(x => x.GetAsync<DateTimeWrapper>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DateTimeWrapper { Value = cachedTime });

            _productsServiceMock
                .Setup(x => x.GetAvailableProducts(It.IsAny<LocationType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            // Act
            var result = await _pricingService.GetSpotPricesAsync(null, side);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Enum.GetValues(typeof(LocationType)).Length, result.Count());
        }

        [Fact]
        public async Task GetSpotPricesAsync_ShouldReturnPricesForGivenLocationAndSide()
        {
            // Arrange
            var location = LocationType.SLC;
            var side = SideType.Buy;

            var product = Product.Create(
                "Generic Product",
                new SKU($"SKU_{location}_{side}"),
                new Weight(1),
                MetalType.XAU,
                true);

            // Add configurations for all locations
            foreach (var loc in Enum.GetValues(typeof(LocationType)).Cast<LocationType>())
            {
                product.AddLocationConfiguration(
                    ProductLocationConfiguration.Create(
                        loc,
                        PremiumUnitType.Dollars,
                        new Premium(10.0m),
                        new Premium(12.0m),
                        true,
                        true));
            }

            var products = new List<Product> { product };

            var cachedSpotPrices = new QuoteResponse
            {
                QuoteKey = "cachedKey",
                QuoteProductsPricingList =
                [
                    new()
                    {
                        ProductCode = product.MetalType.ToAMarkSpotDeferredProductCode(),
                        SpotPrice = 1800.50m
                    }
                ]
            };

            var cachedTime = DateTime.UtcNow;

            _cacheServiceMock
                .Setup(x => x.GetAsync<QuoteResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedSpotPrices);

            _cacheServiceMock
                .Setup(x => x.GetAsync<DateTimeWrapper>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DateTimeWrapper { Value = cachedTime });

            _productsServiceMock
                .Setup(x => x.GetAvailableProducts(location, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            // Act
            var result = await _pricingService.GetSpotPricesAsync(location, side);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            var productPrice = result.First();
            Assert.Equal($"SKU_{location}_{side}", productPrice.ProductSKU);
            Assert.Equal(1800.50m, productPrice.SpotPricePerOz);
        }
    }
}
