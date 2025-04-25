using System.Linq.Expressions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Heding.SpotDeferredTrades.Services;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Application.Trading.Pricing.Services;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.RequestQuote;
using PreciousMetalsTradingSystem.Application.Common.Options;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;
using MockQueryable;
using Microsoft.Extensions.Options;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Models;
using PreciousMetalsTradingSystem.Application.Trading.Pricing.Models;
using PreciousMetalsTradingSystem.Application.Inventory.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Financials.Exceptions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
//using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Execution.Handlers.Commands
{
    public class QuoteRequestCommandHandlerTests
    {
        private readonly Mock<IRepository<TradeQuote, TradeQuoteId>> _tradeQuoteRepositoryMock;
        private readonly Mock<IRepository<Product, ProductId>> _productRepositoryMock;
        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly Mock<IFinancialsService> _financialsServiceMock;
        private readonly Mock<IHedgingService> _hedgingServiceMock;
        private readonly Mock<IPricingService> _pricingServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IOptions<ApiSettingsOptions>> _apiSettingOptionsMock;

        private readonly QuoteRequestCommandHandler _handler;

        public QuoteRequestCommandHandlerTests()
        {
            _tradeQuoteRepositoryMock = new Mock<IRepository<TradeQuote, TradeQuoteId>>();
            _productRepositoryMock = new Mock<IRepository<Product, ProductId>>();
            _inventoryServiceMock = new Mock<IInventoryService>();
            _financialsServiceMock = new Mock<IFinancialsService>();
            _hedgingServiceMock = new Mock<IHedgingService>();
            _pricingServiceMock = new Mock<IPricingService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _apiSettingOptionsMock = new Mock<IOptions<ApiSettingsOptions>>();
            _apiSettingOptionsMock
                .Setup(repo => repo.Value)
                .Returns(new ApiSettingsOptions
                {
                    DefaultPaginationPageSize = 50,
                    UseMockAMarkTradingService = true,
                    SpotPricesRefreshCacheFrequencyInMinutes = 5,
                    QuoteValidityPeriodInSeconds = 25,
                }
            );

            _handler = new QuoteRequestCommandHandler(
                _tradeQuoteRepositoryMock.Object,
                _productRepositoryMock.Object,
                _inventoryServiceMock.Object,
                _financialsServiceMock.Object,
                _hedgingServiceMock.Object,
                _pricingServiceMock.Object,
                _unitOfWorkMock.Object,
                _apiSettingOptionsMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldGetQuote_WhenCommandIsValid()
        {
            // Arrange
            var request = new QuoteRequestCommand
            {
                Location = LocationType.SLC,
                SideType = ClientSideType.Sell,
                Items = new List<QuoteRequestItem>
                {
                    new QuoteRequestItem
                    {
                        ProductSKU = "TST-1",
                        QuantityUnits = 7,
                    }
                }
            };

            var locationConfiguraionSLC = ProductLocationConfiguration.Create(LocationType.SLC, PremiumUnitType.Dollars, new Premium(2.0m), new Premium(2.3m), true, true);
            var locationConfiguraionNY = ProductLocationConfiguration.Create(LocationType.NY, PremiumUnitType.Dollars, new Premium(2.2m), new Premium(2.5m), true, true);

            var product1 = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(1.0m), MetalType.XAU, true);
            product1.AddLocationConfiguration(locationConfiguraionSLC);
            product1.AddLocationConfiguration(locationConfiguraionNY);
            var product2 = Product.Create("Test Product 2", new SKU("TST-2"), new Weight(100.0m), MetalType.XAG, true);
            product2.AddLocationConfiguration(locationConfiguraionSLC);
            product2.AddLocationConfiguration(locationConfiguraionNY);

            var productPrice1 = new ProductPrice
            {
                ProductSKU = product1.SKU,
                ProductName = product1.Name,
                Location = request.Location,
                Side = request.SideType,
                TimestampUtc = DateTime.UtcNow,
                IsAvaiable = true,
                WeightInOz = product1.WeightInOz,
                SpotPricePerOz = 1000m,
                PremiumUnitType = PremiumUnitType.Dollars,
                PremiumPerOz = 10m,
            };
            var productPrice2 = new ProductPrice
            {
                ProductSKU = product2.SKU,
                ProductName = product2.Name,
                Location = request.Location,
                Side = request.SideType,
                TimestampUtc = DateTime.UtcNow,
                IsAvaiable = true,
                WeightInOz = product2.WeightInOz,
                SpotPricePerOz = 2000m,
                PremiumUnitType = PremiumUnitType.Dollars,
                PremiumPerOz = 20m,
            };

            var mockProducts = new List<Product> { product1, product2 };
            _productRepositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(mockProducts.AsQueryable().BuildMock());

            var mockPrices = new List<ProductPrice> { productPrice1, productPrice2 };
            _pricingServiceMock
                .Setup(repo => repo.GetSpotPricesAsync(It.IsAny<LocationType>(), It.IsAny<SideType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockPrices.AsQueryable().BuildMock());

            _financialsServiceMock
                .Setup(repo => repo.HasEnoughCashForBuyAsync(It.IsAny<BalanceType>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var mockData = new GetHedgeQuoteResult
            {
                QuoteKey = "",
                SpotPricesPerOz = new Dictionary<MetalType, decimal>()
                {
                    { product1.MetalType, productPrice1.SpotPricePerOz }
                }
            };
            _hedgingServiceMock
                .Setup(uow => uow.GetHedgeQuoteAsync(It.IsAny<Dictionary<MetalType, QuantityOunces>>(), It.IsAny<SideType>(), It.IsAny<LocationType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockData);

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsType<Quote>(result);
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task Handle_ShouldThrowsNotEnoughQuantityForSellException_WhenInventoryIsInsufficient()
        {
            // Arrange
            var request = new QuoteRequestCommand
            {
                Location = LocationType.SLC,
                SideType = ClientSideType.Buy,
                Items = new List<QuoteRequestItem>
                {
                    new QuoteRequestItem
                    {
                        ProductSKU = "TST-1",
                        QuantityUnits = 7,
                    }
                }
            };



            var locationConfiguraionSLC = ProductLocationConfiguration.Create(LocationType.SLC, PremiumUnitType.Dollars, new Premium(2.0m), new Premium(2.3m), true, true);
            var locationConfiguraionNY = ProductLocationConfiguration.Create(LocationType.NY, PremiumUnitType.Dollars, new Premium(2.2m), new Premium(2.5m), true, true);

            var product1 = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(1.0m), MetalType.XAU, true);
            product1.AddLocationConfiguration(locationConfiguraionSLC);
            product1.AddLocationConfiguration(locationConfiguraionNY);

            var product2 = Product.Create("Test Product 2", new SKU("TST-2"), new Weight(100.0m), MetalType.XAG, true);
            product2.AddLocationConfiguration(locationConfiguraionSLC);
            product2.AddLocationConfiguration(locationConfiguraionNY);

            var mockProducts = new List<Product> { product1, product2 };



            _productRepositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(mockProducts.AsQueryable().BuildMock());


            _inventoryServiceMock
            .Setup(service => service.HasEnoughQuantityForSellAsync(
                It.IsAny<LocationType>(),
                It.IsAny<PositionType>(),
                It.IsAny<Dictionary<ProductId, QuantityUnits>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<NotEnoughQuantityForSellException>(() =>
                _handler.Handle(request, CancellationToken.None));


        }
        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenProductsNotFound()
        {
            // Arrange
            var request = new QuoteRequestCommand
            {
                Location = LocationType.SLC,
                SideType = ClientSideType.Buy,
                Items = new List<QuoteRequestItem>
                {
                    new QuoteRequestItem
                    {
                        ProductSKU = "INVALID-SKU",
                        QuantityUnits = 10
                    }
                }
            };

            _productRepositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<Product>().AsQueryable().BuildMock());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(request, CancellationToken.None));

            Assert.NotNull(exception);

            var errorMessages = exception.Errors["Products"];
            Assert.Contains("Product with SKU 'INVALID-SKU' is not found.", errorMessages);

            _productRepositoryMock.Verify(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenNotEnoughCashBalance()
        {
            // Arrange
            var request = new QuoteRequestCommand
            {
                Location = LocationType.SLC,
                SideType = ClientSideType.Sell,
                Items = new List<QuoteRequestItem>
                {
                    new QuoteRequestItem
                    {
                        ProductSKU = "TST-1",
                        QuantityUnits = 7,
                    }
                }
            };

            var locationConfiguraionSLC = ProductLocationConfiguration.Create(LocationType.SLC, PremiumUnitType.Dollars, new Premium(2.0m), new Premium(2.3m), true, true);
            var locationConfiguraionNY = ProductLocationConfiguration.Create(LocationType.NY, PremiumUnitType.Dollars, new Premium(2.2m), new Premium(2.5m), true, true);

            var product1 = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(1.0m), MetalType.XAU, true);
            product1.AddLocationConfiguration(locationConfiguraionSLC);
            product1.AddLocationConfiguration(locationConfiguraionNY);

            var product2 = Product.Create("Test Product 2", new SKU("TST-2"), new Weight(100.0m), MetalType.XAG, true);
            product2.AddLocationConfiguration(locationConfiguraionSLC);
            product2.AddLocationConfiguration(locationConfiguraionNY);

            var mockProducts = new List<Product> { product1, product2 };

            var productPrice1 = new ProductPrice
            {
                ProductSKU = product1.SKU,
                ProductName = product1.Name,
                Location = request.Location,
                Side = request.SideType,
                TimestampUtc = DateTime.UtcNow,
                IsAvaiable = true,
                WeightInOz = product1.WeightInOz,
                SpotPricePerOz = 1000m,
                PremiumUnitType = PremiumUnitType.Dollars,
                PremiumPerOz = 10m,
            };
            var productPrice2 = new ProductPrice
            {
                ProductSKU = product2.SKU,
                ProductName = product2.Name,
                Location = request.Location,
                Side = request.SideType,
                TimestampUtc = DateTime.UtcNow,
                IsAvaiable = true,
                WeightInOz = product2.WeightInOz,
                SpotPricePerOz = 2000m,
                PremiumUnitType = PremiumUnitType.Dollars,
                PremiumPerOz = 20m,
            };

            _productRepositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(mockProducts.AsQueryable().BuildMock());

            var mockPrices = new List<ProductPrice> { productPrice1, productPrice2 };
            _pricingServiceMock
                .Setup(repo => repo.GetSpotPricesAsync(It.IsAny<LocationType>(), It.IsAny<SideType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockPrices.AsQueryable().BuildMock());

            _financialsServiceMock
                .Setup(x => x.HasEnoughCashForBuyAsync(It.IsAny<BalanceType>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotEnoughCashForBuyException>(() =>
                _handler.Handle(request, CancellationToken.None));

            Assert.NotNull(exception);
            Assert.Contains("There is no enough cash for buy of balance type Effective", exception.Message);
        }
    }


}
