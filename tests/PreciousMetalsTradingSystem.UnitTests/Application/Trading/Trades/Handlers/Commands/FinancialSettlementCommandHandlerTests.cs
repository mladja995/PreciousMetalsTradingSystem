using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Application.Financials.Settlement.Commands;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;


namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Trades.Handlers.Commands
{
    public class FinancialSettlementCommandHandlerTests
    {
        private readonly Mock<IRepository<Trade, TradeId>> _mockTradeRepository;
        private readonly Mock<IRepository<FinancialTransaction, FinancialTransactionId>> _mockFinancialTransactionRepository;
        private readonly Mock<IFinancialsService> _mockFinancialService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<FinancialSettlementCommand>> _mockLogger;
        private readonly FinancialSettlementCommandHandler _handler;

        public FinancialSettlementCommandHandlerTests()
        {
            _mockTradeRepository = new Mock<IRepository<Trade, TradeId>>();
            _mockFinancialTransactionRepository = new Mock<IRepository<FinancialTransaction, FinancialTransactionId>>();
            _mockFinancialService = new Mock<IFinancialsService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<FinancialSettlementCommand>>();

            _handler = new FinancialSettlementCommandHandler(
                _mockTradeRepository.Object,
                _mockFinancialService.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateFinancialTransactionsSuccessfully()
        {
            // Arrange
            var trade = Trade.Create(
                    TradeType.ClientTrade,
                    SideType.Buy,
                    LocationType.SLC,
                    DateOnly.FromDateTime(DateTime.UtcNow),
                    DateOnly.FromDateTime(DateTime.UtcNow),
                    "Test note");

            var trades = new[] { trade };

            var financialTransaction = FinancialTransaction.Create(
                trade.Side.ToTransactionSideType(), 
                BalanceType.Actual, 
                trade.Type.ToActivityType(), 
                trade.GetTotalAmount(), 
                new FinancialBalance(5000),
                trade.Id);

            _mockTradeRepository
                .Setup(repo => repo.StartQuery(false, true))
                .Returns(new[] { trade }.AsQueryable().BuildMock());


            _mockFinancialService
                .Setup(service => service.CreateFinancialTransactionAsync(
                    It.IsAny<ActivityType>(),
                    It.IsAny<TransactionSideType>(),
                    It.IsAny<BalanceType>(),
                    It.IsAny<Money>(),
                    It.IsAny<IEntityId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(financialTransaction);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));

            // Act
            await _handler.Handle(new FinancialSettlementCommand(), CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowFinancialSettlementJobException_WhenFails()
        {
            // Arrange
            var trade = Trade.Create(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.SLC,
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow),
                "Test note"
            );
            var trade1 = Trade.Create(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.SLC,
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow),
                "Test note"
            );

            var trades = new[] { trade, trade1 };


            var existingFinancialTransaction = FinancialTransaction.Create(
                trade.Side.ToTransactionSideType(), 
                BalanceType.Actual, 
                trade.Type.ToActivityType(), 
                trade.GetTotalAmount(), 
                new FinancialBalance(5000),
                trade.Id);

            trade.AddFinancialTransaction(existingFinancialTransaction);

            var newFinancialTransaction = FinancialTransaction.Create(
                trade.Side.ToTransactionSideType(), 
                BalanceType.Actual, 
                trade.Type.ToActivityType(), 
                trade.GetTotalAmount(), 
                new FinancialBalance(5000),
                trade.Id);

            _mockTradeRepository
                .Setup(repo => repo.StartQuery(false, true))
                .Returns(trades.AsQueryable().BuildMock());

            _mockFinancialService
                .Setup(service => service.CreateFinancialTransactionAsync(
                    It.IsAny<ActivityType>(),
                    It.IsAny<TransactionSideType>(),
                    It.IsAny<BalanceType>(),
                    It.IsAny<Money>(),
                    It.IsAny<IEntityId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(newFinancialTransaction);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));


            // Act & Assert
            var exception = await Assert.ThrowsAsync<FinancialSettlementJobException>(() => _handler.Handle(new FinancialSettlementCommand(), CancellationToken.None));

            // Assert
            Assert.Contains("The following trades failed to process", exception.Message);
            Assert.Contains(trade.Id.ToString(), exception.Message);
        }
    }
}
