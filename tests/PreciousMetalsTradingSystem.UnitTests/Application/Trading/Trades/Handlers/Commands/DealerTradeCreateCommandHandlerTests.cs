using System.Linq.Expressions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Heding.SpotDeferredTrades.Services;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Services;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Application.Trading.Pricing.Models;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create;
using Models = PreciousMetalsTradingSystem.Application.Trading.Trades.Models;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;
using MockQueryable;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Financials.Exceptions;
using PreciousMetalsTradingSystem.Application.Inventory.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;
using PreciousMetalsTradingSystem.Application.Trading.Services;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Trades.Handlers.Commands
{
    public class DealerTradeCreateCommandHandlerTests
    {
        private readonly Mock<IRepository<Trade, TradeId>> _tradingRepositoryMock;
        private readonly Mock<IRepository<SpotDeferredTrade, SpotDeferredTradeId>> _spotDeferredTradeRepositoryMock;
        private readonly Mock<IRepository<Product, ProductId>> _productRepositoryMock;
        private readonly Mock<IRepository<LocationHedgingAccountConfiguration, LocationHedgingAccountConfigurationId>> _locationHedgingAccountRepositoryMock;
        private readonly Mock<IRepository<FinancialTransaction, FinancialTransactionId>> _financialTransactionRepositoryMock;
        private readonly Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>> _productLocationPositionRespositoryMock;
        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly Mock<IFinancialsService> _financialsServiceMock;
        private readonly Mock<IHedgingService> _hedgingServiceMock;
        private readonly Mock<ICalendarService> _calendarServiceMock;
        private readonly Mock<ITradingService> _tradingServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly TradeCreateCommandHandler _handler;

        public DealerTradeCreateCommandHandlerTests()
        {
            _tradingRepositoryMock = new Mock<IRepository<Trade, TradeId>>();
            _spotDeferredTradeRepositoryMock = new Mock<IRepository<SpotDeferredTrade, SpotDeferredTradeId>>();
            _productRepositoryMock = new Mock<IRepository<Product, ProductId>>();
            _locationHedgingAccountRepositoryMock = new Mock<IRepository<LocationHedgingAccountConfiguration, LocationHedgingAccountConfigurationId>>();
            _financialTransactionRepositoryMock = new Mock<IRepository<FinancialTransaction, FinancialTransactionId>>();
            _productLocationPositionRespositoryMock = new Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>>();
            _inventoryServiceMock = new Mock<IInventoryService>();
            _financialsServiceMock = new Mock<IFinancialsService>();
            _hedgingServiceMock = new Mock<IHedgingService>();
            _calendarServiceMock = new Mock<ICalendarService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _tradingServiceMock = new Mock<ITradingService>();

            _handler = new TradeCreateCommandHandler(
                _tradingRepositoryMock.Object,
                _spotDeferredTradeRepositoryMock.Object,
                _productRepositoryMock.Object,
                _locationHedgingAccountRepositoryMock.Object,
                _financialTransactionRepositoryMock.Object,
                _productLocationPositionRespositoryMock.Object,
                _inventoryServiceMock.Object,
                _financialsServiceMock.Object,
                _hedgingServiceMock.Object,
                _calendarServiceMock.Object,
                _unitOfWorkMock.Object,
                _tradingServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldCreateAndSaveTrade_WhenCommandIsValid()
        {
            // Arrange
            var product1 = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(32.148m), MetalType.XAU, true);
            var productLocationConfiguraion11 = ProductLocationConfiguration.Create(LocationType.SLC, PremiumUnitType.Dollars, new Premium(0.75m), new Premium(1.75m), true, true);
            product1.AddLocationConfiguration(productLocationConfiguraion11);
            var productLocationConfiguraion12 = ProductLocationConfiguration.Create(LocationType.NY, PremiumUnitType.Dollars, new Premium(-1.2m), new Premium(1.0m), true, true);
            product1.AddLocationConfiguration(productLocationConfiguraion12);

            var product2 = Product.Create("Test Product 2", new SKU("TST-2"), new Weight(1.0m), MetalType.XAU, true);
            var productLocationConfiguraion21 = ProductLocationConfiguration.Create(LocationType.SLC, PremiumUnitType.Dollars, new Premium(-0.4m), new Premium(0.85m), true, true);
            product2.AddLocationConfiguration(productLocationConfiguraion21);
            var productLocationConfiguraion22 = ProductLocationConfiguration.Create(LocationType.NY, PremiumUnitType.Dollars, new Premium(0.2m), new Premium(1.0m), true, true);
            product2.AddLocationConfiguration(productLocationConfiguraion22);

            var product3 = Product.Create("Test Product 3", new SKU("TST-3"), new Weight(100m), MetalType.XAG, true);
            var productLocationConfiguraion31 = ProductLocationConfiguration.Create(LocationType.SLC, PremiumUnitType.Dollars, new Premium(-0.3m), new Premium(1.35m), true, true);
            product3.AddLocationConfiguration(productLocationConfiguraion31);
            var productLocationConfiguraion32 = ProductLocationConfiguration.Create(LocationType.NY, PremiumUnitType.Dollars, new Premium(0.8m), new Premium(2.0m), true, true);
            product3.AddLocationConfiguration(productLocationConfiguraion32);

            _tradingRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()));

            _spotDeferredTradeRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<SpotDeferredTrade>(), It.IsAny<CancellationToken>()));

            var productMockData = new List<Product> { product1, product2, product3 };
            _productRepositoryMock
                .Setup(repo => repo.StartQuery(false, false))
                .Returns(productMockData.AsQueryable().BuildMock());

            var hedgingAccount = HedgingAccount.Create(new HedgingAccountName("Test Account"), new HedgingAccountCode("Code"));
            var hedgingAccountLocationConfiguration = LocationHedgingAccountConfiguration.Create(LocationType.SLC,hedgingAccount.Id);
            typeof(LocationHedgingAccountConfiguration).GetProperty("HedgingAccount")!.SetValue(hedgingAccountLocationConfiguration, hedgingAccount);

            _locationHedgingAccountRepositoryMock
                .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<LocationHedgingAccountConfigurationId>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<LocationHedgingAccountConfiguration, object>>[]>()))
                .ReturnsAsync(hedgingAccountLocationConfiguration);

            _financialTransactionRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<FinancialTransaction>(), It.IsAny<CancellationToken>()));

            _productLocationPositionRespositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<ProductLocationPosition>(), It.IsAny<CancellationToken>()));

            _inventoryServiceMock
                .Setup(x => x.HasEnoughQuantityForSellAsync(It.IsAny<LocationType>(), It.IsAny<PositionType>(), It.IsAny<Dictionary<ProductId, QuantityUnits>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _inventoryServiceMock
                .Setup(x => x.CreatePositionAsync(It.IsAny<ProductId>(), It.IsAny<TradeId>(), It.IsAny<LocationType>(), It.IsAny<PositionType>(), It.IsAny<PositionSideType>(), It.IsAny<QuantityUnits>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductLocationPosition());

            _financialsServiceMock
                .Setup(x => x.HasEnoughCashForBuyAsync(It.IsAny<BalanceType>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _financialsServiceMock
                .Setup(x => x.CreateFinancialTransactionAsync(It.IsAny<ActivityType>(), It.IsAny<TransactionSideType>(), It.IsAny<BalanceType>(), It.IsAny<Money>(), It.IsAny<IEntityId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialTransaction());

            var hedgeResult = new HedgeResult
            {
                QuoteKey = "Test Key",
                TradeConfirmationNumber = "Test Number",
                SpotPricesPerOz = new Dictionary<MetalType, decimal>
                {
                    { MetalType.XAU, 1006m }
                }
            };
            _hedgingServiceMock
                .Setup(uow => uow.HedgeAsync(It.IsAny<Dictionary<MetalType, QuantityOunces>>(), It.IsAny<SideType>(), It.IsAny<LocationType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(hedgeResult);

            _calendarServiceMock
                .Setup(uow => uow.AddBusinessDaysAsync(It.IsAny<DateOnly>(), It.IsAny<int>(), It.IsAny<CalendarType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DateTime.UtcNow.ConvertUtcToEstDateOnly().AddDays(2));

            var productPrice1 = new ProductPrice
            {
                ProductSKU = product1.SKU,
                ProductName = product1.Name,
                Location = LocationType.NY,
                Side = SideType.Buy.ToClientSideType(),
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
                Location = LocationType.NY,
                Side = SideType.Buy.ToClientSideType(),
                TimestampUtc = DateTime.UtcNow,
                IsAvaiable = true,
                WeightInOz = product2.WeightInOz,
                SpotPricePerOz = 2000m,
                PremiumUnitType = PremiumUnitType.Dollars,
                PremiumPerOz = 20m,
            };

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new DealerTradeCreateCommand
            {
                TradeDate = DateTime.UtcNow.ConvertUtcToEst(),
                Location = LocationType.NY,
                SideType = SideType.Buy,
                AutoHedge = true,
                Note = "Test note",
                Items =
                [
                    new Models.DealerTradeItemRequest
                    {
                        ProductId = product1.Id.Value,
                        UnitQuantity = 2,
                        DealerPricePerOz = 3680.5m,
                        SpotPricePerOz = null,
                    },
                    new Models.DealerTradeItemRequest
                    {
                        ProductId = product2.Id.Value,
                        UnitQuantity = 7,
                        DealerPricePerOz = 3520.5m,
                        SpotPricePerOz = null,
                    }
                ]
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _tradingRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsType<Guid>(result);
            Assert.NotEqual(Guid.Empty, result);
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenProductDoesNotExist()
        {
            // Arrange
            _productRepositoryMock
                .Setup(repo => repo.StartQuery(false, false))
                .Returns(new List<Product>().AsQueryable().BuildMock());

            var command = new DealerTradeCreateCommand
            {
                TradeDate = DateTime.UtcNow,
                Location = LocationType.SLC,
                SideType = SideType.Buy,
                AutoHedge = false,
                Items = new List<Models.DealerTradeItemRequest>
                {
                    new Models.DealerTradeItemRequest
                    {
                        ProductId = Guid.NewGuid(), 
                        UnitQuantity = 5,
                        DealerPricePerOz = 1500m
                    }
                }
            };

            var productId = command.Items.Select(x => x.ProductId).FirstOrDefault();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.NotNull(exception);

            var errorMessages = exception.Errors["Products"];
            var expectedErrorMessage = $"Product with ID '{productId}' is not found.";
            Assert.Contains(expectedErrorMessage, errorMessages);

            _productRepositoryMock.Verify(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);

        }

        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenNotEnoughCashBalance()
        {
            // Arrange
            var product = Product.Create("Test Product", new SKU("TST-1"), new Weight(32.148m), MetalType.XAU, true);
            product.AddLocationConfiguration(ProductLocationConfiguration.Create(LocationType.NY, PremiumUnitType.Dollars, new Premium(0.5m), new Premium(1.5m), true, true));

            _productRepositoryMock
                .Setup(repo => repo.StartQuery(false, false))
                .Returns(new List<Product> { product }.AsQueryable().BuildMock());

            _financialsServiceMock
                .Setup(x => x.HasEnoughCashForBuyAsync(It.IsAny<BalanceType>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = new DealerTradeCreateCommand
            {
                TradeDate = DateTime.UtcNow,
                Location = LocationType.NY,
                SideType = SideType.Buy,
                AutoHedge = false,
                Items = new List<Models.DealerTradeItemRequest>
                {
                    new Models.DealerTradeItemRequest
                    {
                        ProductId = product.Id.Value,
                        UnitQuantity = 2,
                        DealerPricePerOz = 1500m
                    }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotEnoughCashForBuyException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.NotNull(exception);
            Assert.Contains("There is no enough cash for buy of balance type Effective", exception.Message);

        }

        [Fact]
        public async Task Handle_ShouldThrowsNotEnoughQuantityForSellException_WhenInventoryIsInsufficient()
        {
            var product = Product.Create("Test Product", new SKU("TST-1"), new Weight(32.148m), MetalType.XAU, true);
            product.AddLocationConfiguration(ProductLocationConfiguration.Create(LocationType.NY, PremiumUnitType.Dollars, new Premium(0.5m), new Premium(1.5m), true, true));

            _productRepositoryMock
               .Setup(repo => repo.StartQuery(false, false))
               .Returns(new List<Product> { product }.AsQueryable().BuildMock());

            _inventoryServiceMock
            .Setup(service => service.HasEnoughQuantityForSellAsync(
                It.IsAny<LocationType>(),
                It.IsAny<PositionType>(),
                It.IsAny<Dictionary<ProductId, QuantityUnits>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

            var command = new DealerTradeCreateCommand
            {
                TradeDate = DateTime.UtcNow,
                Location = LocationType.NY,
                SideType = SideType.Sell,
                AutoHedge = false,
                Items = new List<Models.DealerTradeItemRequest>
                {
                    new Models.DealerTradeItemRequest
                    {
                        ProductId = product.Id.Value,
                        UnitQuantity = 2,
                        DealerPricePerOz = 1500m
                    }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotEnoughQuantityForSellException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.NotNull(exception);
            Assert.Contains("There is no enough quantity for sell", exception.Message);

        }
    }
}
