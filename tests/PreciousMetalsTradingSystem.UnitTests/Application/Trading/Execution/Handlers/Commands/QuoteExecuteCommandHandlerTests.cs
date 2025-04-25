using System.Linq.Expressions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Heding.SpotDeferredTrades.Services;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Services;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.ExecuteQuote;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;
using MockQueryable;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Financials.Exceptions;
using PreciousMetalsTradingSystem.Application.Inventory.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Options;
using Microsoft.Extensions.Options;
using PreciousMetalsTradingSystem.Application.Trading.Services;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Execution.Handlers.Commands
{
    public class QuoteExecuteCommandHandlerTests
    {
        private readonly Mock<IRepository<TradeQuote, TradeQuoteId>> _tradeQuoteRepositoryMock;
        private readonly Mock<IRepository<Trade, TradeId>> _tradingRepositoryMock;
        private readonly Mock<IRepository<SpotDeferredTrade, SpotDeferredTradeId>> _spotDeferredTradeRepositoryMock;
        private readonly Mock<IRepository<LocationHedgingAccountConfiguration, LocationHedgingAccountConfigurationId>> _locationHedgingAccountRepositoryMock;
        private readonly Mock<IRepository<FinancialTransaction, FinancialTransactionId>> _financialTransactionRepositoryMock;
        private readonly Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>> _productLocationPositionRespositoryMock;
        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly Mock<IFinancialsService> _financialsServiceMock;
        private readonly Mock<IHedgingService> _hedgingServiceMock;
        private readonly Mock<ICalendarService> _calendarServiceMock;
        private readonly Mock<ITradingService> _tradingServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IOptions<ApiSettingsOptions> _options;

        private readonly QuoteExecuteCommandHandler _handler;

        public QuoteExecuteCommandHandlerTests()
        {
            _tradeQuoteRepositoryMock = new Mock<IRepository<TradeQuote, TradeQuoteId>>();
            _tradingRepositoryMock = new Mock<IRepository<Trade, TradeId>>();
            _spotDeferredTradeRepositoryMock = new Mock<IRepository<SpotDeferredTrade, SpotDeferredTradeId>>();
            _locationHedgingAccountRepositoryMock = new Mock<IRepository<LocationHedgingAccountConfiguration, LocationHedgingAccountConfigurationId>>();
            _financialTransactionRepositoryMock = new Mock<IRepository<FinancialTransaction, FinancialTransactionId>>();
            _productLocationPositionRespositoryMock = new Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>>();
            _inventoryServiceMock = new Mock<IInventoryService>();
            _financialsServiceMock = new Mock<IFinancialsService>();
            _hedgingServiceMock = new Mock<IHedgingService>();
            _calendarServiceMock = new Mock<ICalendarService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _tradingServiceMock = new Mock<ITradingService>();
            _options = Options.Create(new ApiSettingsOptions
            {
                TradeDuplicateLookupPeriodInDays = 7
            });

            _handler = new QuoteExecuteCommandHandler(
                _tradeQuoteRepositoryMock.Object,
                _tradingRepositoryMock.Object,
                _spotDeferredTradeRepositoryMock.Object,
                _locationHedgingAccountRepositoryMock.Object,
                _inventoryServiceMock.Object,
                _financialsServiceMock.Object,
                _hedgingServiceMock.Object,
                _calendarServiceMock.Object,
                _tradingServiceMock.Object,
                _financialTransactionRepositoryMock.Object,
                _productLocationPositionRespositoryMock.Object,
                _unitOfWorkMock.Object,
                _options
            );
        }

        [Fact]
        public async Task Handle_ShouldExecuteQuote_WhenCommandIsValid()
        {
            // Arrange
            var product1 = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(100.0m), MetalType.XAG, true);
            var product2 = Product.Create("Test Product 2", new SKU("TST-2"), new Weight(1.0m), MetalType.XAU, true);
            var product3 = Product.Create("Test Product 3", new SKU("TST-3"), new Weight(32.148m), MetalType.XAG, true);

            var tradeQuote = TradeQuote.Create("ABC-123456", DateTime.UtcNow.AddMinutes(-2), DateTime.UtcNow.AddMinutes(3), SideType.Buy, LocationType.SLC);
            var item1 = TradeQuoteItem.Create(product1, new QuantityUnits(12), new Money(105.75m), new Premium(3.25m), new Money(107.2m));
            var item2 = TradeQuoteItem.Create(product2, new QuantityUnits(4), new Money(2806.25m), new Premium(8.65m), new Money(2831.6m));
            tradeQuote.AddItem(item1);
            tradeQuote.AddItem(item2);

            var mockData = new List<TradeQuote> { tradeQuote };

            _tradeQuoteRepositoryMock
                .Setup(repo => repo.StartQuery(false, false))
                .Returns(mockData.AsQueryable().BuildMock());

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

            _calendarServiceMock
                .Setup(uow => uow.AddBusinessDaysAsync(It.IsAny<DateOnly>(), It.IsAny<int>(), It.IsAny<CalendarType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DateTime.UtcNow.ConvertUtcToEstDateOnly().AddDays(2));

            var hedgeResult = new ExecuteHedgeQuoteResult
            {
                QuoteKey = "Test Key",
                TradeConfirmationNumber = "Test Number",
            };
            _hedgingServiceMock
                .Setup(uow => uow.ExecuteHedgeQuoteAsync(It.IsAny<LocationType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(hedgeResult);

            var hedgingAccount = HedgingAccount.Create(new HedgingAccountName("Test Account"), new HedgingAccountCode("Code"));
            var hedgingAccountLocationConfiguration = LocationHedgingAccountConfiguration.Create(LocationType.SLC, hedgingAccount.Id);
            typeof(LocationHedgingAccountConfiguration).GetProperty("HedgingAccount")!.SetValue(hedgingAccountLocationConfiguration, hedgingAccount);

            _locationHedgingAccountRepositoryMock
                .Setup(x => x.GetByIdOrThrowAsync(It.IsAny<LocationHedgingAccountConfigurationId>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<LocationHedgingAccountConfiguration, object>>[]>()))
                .ReturnsAsync(hedgingAccountLocationConfiguration);

            _spotDeferredTradeRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<SpotDeferredTrade>(), It.IsAny<CancellationToken>()));

            _tradingRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()));

            var command = new QuoteExecuteCommand
            {
                Id = tradeQuote.Id
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _tradingRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()), Times.Once);
            _spotDeferredTradeRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<SpotDeferredTrade>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsType<QuoteExecuteCommandResult>(result);
            Assert.NotEqual(Guid.Empty, result.TradeId);
            Assert.NotEqual(string.Empty, result.TradeNumber);
            Assert.NotNull(result.Quote);
            Assert.Equal(command.Id, result.Quote.Id);
        }
        [Theory]
        [InlineData(false, true, true, false, typeof(NotEnoughCashForBuyException), "There is no enough cash for buy of balance type Effective")]
        [InlineData(true, true, true, true, typeof(TradeQuoteInvalidStatusException), $"Trade Quote in wrong state (Executed)!")]
        [InlineData(true, true, false, false, typeof(TradeQuoteExpiredException), "Trade Quote expired!")]
        [InlineData(true, true, true, false, typeof(NotFoundException), $"Entity {nameof(TradeQuote)} with key '**Id**' was not found.")]
        public async Task Handle_ShouldThrowException_WhenValidationFails(
            bool hasEnoughCash,
            bool hasEnoughQuantity,
            bool isQuoteValid,
            bool canExecute,
            Type expectedExceptionType,
            string expectedExceptionMessage)
        {
            // Arrange
            var tradeQuote = TradeQuote.Create(
                dealerQuoteId: "FAIL-123456",
                issuedAtUtc: DateTime.UtcNow.AddMinutes(-10),
                expiresAtUtc: isQuoteValid ? DateTime.UtcNow.AddMinutes(3) : DateTime.UtcNow.AddMinutes(-1),
                side: SideType.Buy,
                locationType: LocationType.SLC);
            if (canExecute)
            {
                tradeQuote.MarkAsExecuted();
            }

            var product1 = Product.Create("Gold", new SKU("GLD"), new Weight(1.0m), MetalType.XAU, true);
            var item1 = TradeQuoteItem.Create(product1, new QuantityUnits(2), new Money(2000m), new Premium(50m), new Money(2050m));
            tradeQuote.AddItem(item1);

            _tradeQuoteRepositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<TradeQuote> { tradeQuote }.AsQueryable().BuildMock());

            _financialsServiceMock
                .Setup(f => f.HasEnoughCashForBuyAsync(It.IsAny<BalanceType>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(hasEnoughCash);

            _inventoryServiceMock
                .Setup(i => i.HasEnoughQuantityForSellAsync(It.IsAny<LocationType>(), It.IsAny<PositionType>(), It.IsAny<Dictionary<ProductId, QuantityUnits>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(hasEnoughQuantity);

            var command = new QuoteExecuteCommand
            {
                Id = expectedExceptionMessage.Contains("**Id**") ? Guid.NewGuid() : tradeQuote.Id
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync(expectedExceptionType, () => _handler.Handle(command, CancellationToken.None));

            Assert.Equal(expectedExceptionMessage.Replace("**Id**", command.Id.ToString()), exception.Message);

            // Verify no downstream operations occurred
            _financialsServiceMock.Verify(f => f.CreateFinancialTransactionAsync(It.IsAny<ActivityType>(), It.IsAny<TransactionSideType>(), It.IsAny<BalanceType>(), It.IsAny<Money>(), It.IsAny<IEntityId>(), It.IsAny<CancellationToken>()), Times.Never);
            _inventoryServiceMock.Verify(i => i.CreatePositionAsync(It.IsAny<ProductId>(), It.IsAny<TradeId>(), It.IsAny<LocationType>(), It.IsAny<PositionType>(), It.IsAny<PositionSideType>(), It.IsAny<QuantityUnits>(), It.IsAny<CancellationToken>()), Times.Never);
            _hedgingServiceMock.Verify(h => h.ExecuteHedgeQuoteAsync(It.IsAny<LocationType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task Handle_ShouldThrowNotEnoughQuantityForSellException_WhenInsufficientQuantityForSell()
        {
            // Arrange
            var tradeQuote = TradeQuote.Create(
                dealerQuoteId: "SELL-123456",
                issuedAtUtc: DateTime.UtcNow.AddMinutes(-5),
                expiresAtUtc: DateTime.UtcNow.AddMinutes(10),
                side: SideType.Sell, // Selling side
                locationType: LocationType.NY);

            var product1 = Product.Create("Silver", new SKU("SLV"), new Weight(50.0m), MetalType.XAG, true);
            var item1 = TradeQuoteItem.Create(product1, new QuantityUnits(100), new Money(500), new Premium(20), new Money(520)); // Requested 100 units
            tradeQuote.AddItem(item1);

            _tradeQuoteRepositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<TradeQuote> { tradeQuote }.AsQueryable().BuildMock());

            // Mock inventory service to return insufficient quantity
            _inventoryServiceMock
                .Setup(i => i.HasEnoughQuantityForSellAsync(
                    It.IsAny<LocationType>(),
                    It.IsAny<PositionType>(),
                    It.Is<Dictionary<ProductId, QuantityUnits>>(q => q.ContainsKey(product1.Id) && q[product1.Id].Value == 100),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // Not enough quantity

            var command = new QuoteExecuteCommand
            {
                Id = tradeQuote.Id
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotEnoughQuantityForSellException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Equal($"There is no enough quantity for sell of position type {PositionType.AvailableForTrading}", exception.Message);

            // Verify no downstream operations occurred
            _financialsServiceMock.Verify(f => f.CreateFinancialTransactionAsync(It.IsAny<ActivityType>(), It.IsAny<TransactionSideType>(), It.IsAny<BalanceType>(), It.IsAny<Money>(), It.IsAny<IEntityId>(), It.IsAny<CancellationToken>()), Times.Never);
            _inventoryServiceMock.Verify(i => i.CreatePositionAsync(It.IsAny<ProductId>(), It.IsAny<TradeId>(), It.IsAny<LocationType>(), It.IsAny<PositionType>(), It.IsAny<PositionSideType>(), It.IsAny<QuantityUnits>(), It.IsAny<CancellationToken>()), Times.Never);
            _hedgingServiceMock.Verify(h => h.ExecuteHedgeQuoteAsync(It.IsAny<LocationType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task Handle_ShouldThrowTradeReferenceNumberIsNotUniqueException_WhenTradeWithSameReferenceExists()
        {
            // Arrange
            var tradeQuote = TradeQuote.Create(
                dealerQuoteId: "BUY-12345678",
                issuedAtUtc: DateTime.UtcNow.AddMinutes(-3),
                expiresAtUtc: DateTime.UtcNow.AddMinutes(5),
                side: SideType.Buy,
                locationType: LocationType.SLC);
            var referenceNumber = "DUPLICATE-REF";
            var product = Product.Create("Gold", new SKU("GLD"), new Weight(1.0m), MetalType.XAU, true);
            var item = TradeQuoteItem.Create(product, new QuantityUnits(5), new Money(2000m), new Premium(50m), new Money(2050m));
            tradeQuote.AddItem(item);

            var duplicateTrade = Trade.Create(TradeType.ClientTrade, SideType.Buy, LocationType.SLC, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), "Test note", referenceNumber);

            _tradeQuoteRepositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<TradeQuote> { tradeQuote }.AsQueryable().BuildMock());

            _tradingRepositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<Trade> { duplicateTrade }.AsQueryable().BuildMock());

            _financialsServiceMock
                .Setup(f => f.HasEnoughCashForBuyAsync(It.IsAny<BalanceType>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _inventoryServiceMock
                .Setup(i => i.HasEnoughQuantityForSellAsync(
                    It.IsAny<LocationType>(),
                    It.IsAny<PositionType>(),
                    It.IsAny<Dictionary<ProductId, QuantityUnits>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new QuoteExecuteCommand
            {
                Id = tradeQuote.Id,
                ReferenceNumber = "DUPLICATE-REF"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TradeReferenceNumberIsNotUniqueException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Equal($"A trade with reference number: '{"DUPLICATE-REF"}' already exists.", ex.Message);

            _tradingRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}