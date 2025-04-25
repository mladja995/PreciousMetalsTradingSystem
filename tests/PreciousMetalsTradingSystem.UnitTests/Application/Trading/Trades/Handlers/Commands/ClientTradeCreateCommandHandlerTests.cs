using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Services;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Models;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;
using MockQueryable;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Financials.Exceptions;
using PreciousMetalsTradingSystem.Application.Inventory.Exceptions;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Application.Heding.SpotDeferredTrades.Services;
using PreciousMetalsTradingSystem.Application.Trading.Services;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Trades.Handlers.Commands
{
    public class ClientTradeCreateCommandHandlerTests
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

        public ClientTradeCreateCommandHandlerTests()
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
            var product = Product.Create("Gold Bar", new SKU("GB-001"), new Weight(32.148m), MetalType.XAU, true);
            product.AddLocationConfiguration(ProductLocationConfiguration.Create(LocationType.NY, PremiumUnitType.Dollars, new Premium(1.0m), new Premium(2.0m), true, true));

            _productRepositoryMock
                .Setup(repo => repo.StartQuery(false, false))
                .Returns(new List<Product> { product }.AsQueryable().BuildMock());

            _financialsServiceMock
                .Setup(x => x.HasEnoughCashForBuyAsync(It.IsAny<BalanceType>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _calendarServiceMock
                .Setup(uow => uow.AddBusinessDaysAsync(It.IsAny<DateOnly>(), It.IsAny<int>(), It.IsAny<CalendarType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)));

            _financialsServiceMock
            .Setup(service => service.CreateFinancialTransactionAsync(
                It.IsAny<ActivityType>(),
                It.IsAny<TransactionSideType>(),
                It.IsAny<BalanceType>(),
                It.IsAny<Money>(),
                It.IsAny<IEntityId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FinancialTransaction());

            _inventoryServiceMock
                .Setup(service => service.CreatePositionAsync(
                    It.IsAny<ProductId>(),
                    It.IsAny<TradeId>(),
                    It.IsAny<LocationType>(),
                    It.IsAny<PositionType>(),
                    It.IsAny<PositionSideType>(),
                    It.IsAny<QuantityUnits>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductLocationPosition());

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new ClientTradeCreateCommand
            {
                TradeDate = DateTime.UtcNow,
                Location = LocationType.NY,
                SideType = ClientSideType.Sell,
                Items = new List<ClientTradeItemRequest>
                {
                    new ClientTradeItemRequest
                    {
                        ProductId = product.Id.Value,
                        UnitQuantity = 2,
                        SpotPricePerOz = 2000m
                    }
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _tradingRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenProductDoesNotExist()
        {
            // Arrange
            _productRepositoryMock
                .Setup(repo => repo.StartQuery(false, false))
                .Returns(new List<Product>().AsQueryable().BuildMock());

            var command = new ClientTradeCreateCommand
            {
                TradeDate = DateTime.UtcNow,
                Location = LocationType.SLC,
                SideType = ClientSideType.Buy,
                Items = new List<ClientTradeItemRequest>
                {
                    new ClientTradeItemRequest
                    {
                        ProductId = Guid.NewGuid(),
                        UnitQuantity = 5,
                        SpotPricePerOz = 1500m
                    }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowNotEnoughCashException_WhenInsufficientFunds()
        {
            // Arrange
            var product = Product.Create("Silver Bar", new SKU("SB-001"), new Weight(10m), MetalType.XAG, true);
            product.AddLocationConfiguration(ProductLocationConfiguration.Create(LocationType.NY, PremiumUnitType.Dollars, new Premium(1.0m), new Premium(2.0m), true, true));

            _productRepositoryMock
                .Setup(repo => repo.StartQuery(false, false))
                .Returns(new List<Product> { product }.AsQueryable().BuildMock());

            _financialsServiceMock
                .Setup(x => x.HasEnoughCashForBuyAsync(It.IsAny<BalanceType>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = new ClientTradeCreateCommand
            {
                TradeDate = DateTime.UtcNow,
                Location = LocationType.NY,
                SideType = ClientSideType.Sell,
                Items = new List<ClientTradeItemRequest>
                {
                    new ClientTradeItemRequest
                    {
                        ProductId = product.Id.Value,
                        UnitQuantity = 3,
                        SpotPricePerOz = 1200m
                    }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotEnoughCashForBuyException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowsNotEnoughQuantityForSellException_WhenInventoryIsInsufficient()
        {
            // Arrange
            var product = Product.Create("Gold Bar", new SKU("GB-001"), new Weight(32.148m), MetalType.XAU, true);
            product.AddLocationConfiguration(ProductLocationConfiguration.Create(LocationType.NY, PremiumUnitType.Dollars, new Premium(1.0m), new Premium(2.0m), true, true));

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

            var command = new ClientTradeCreateCommand
            {
                TradeDate = DateTime.UtcNow,
                Location = LocationType.NY,
                SideType = ClientSideType.Buy,
                Items = new List<ClientTradeItemRequest>
                {
                    new ClientTradeItemRequest
                    {
                        ProductId = product.Id.Value,
                        UnitQuantity = 3,
                        SpotPricePerOz = 1800m
                    }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotEnoughQuantityForSellException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }
}

