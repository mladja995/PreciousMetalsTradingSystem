using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Queries.GetCurrentBalance;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MockQueryable;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Financial.Handlers.Queries
{
    public class GetCurrentBalanceQueryHandlerTests
    {
        private readonly Mock<IRepository<FinancialTransaction, FinancialTransactionId>> _transactionRepositoryMock;
        private readonly GetCurrentBalanceQueryHandler _handler;

        public GetCurrentBalanceQueryHandlerTests()
        {
            _transactionRepositoryMock = new Mock<IRepository<FinancialTransaction, FinancialTransactionId>>();
            _handler = new GetCurrentBalanceQueryHandler(_transactionRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectBalance_WhenDataExists()
        {
            // Arrange

            var transaction1 = FinancialTransaction.Create(
            TransactionSideType.Credit,
            BalanceType.Effective,
            ActivityType.DealerTrade,
            new Money(100m),
            new FinancialBalance(500m),
            TradeId.New());

            var transaction2 = FinancialTransaction.Create(
            TransactionSideType.Credit,
            BalanceType.Actual,
            ActivityType.DealerTrade,
            new Money(100m),
            new FinancialBalance(300m),
            TradeId.New());

            var transactions = new List<FinancialTransaction> { transaction1, transaction2};

            _transactionRepositoryMock.Setup(r => r.StartQuery(true, true)).Returns(new[] { transaction1, transaction2 }.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(new GetCurrentBalanceQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(400, result.Actual);
            Assert.Equal(600, result.AvailableForTrading);
        }

        [Fact]
        public async Task Handle_ReturnsZeroBalance_WhenNoDataExists()
        {
            _transactionRepositoryMock.Setup(r => r.StartQuery(true, true)).Returns(new FinancialTransaction[] { }.AsQueryable().BuildMock());

            var result = await _handler.Handle(new GetCurrentBalanceQuery(), CancellationToken.None);

            Assert.Equal(0, result.Actual);
            Assert.Equal(0, result.AvailableForTrading);
        }
        [Fact]
        public async Task Handle_ReturnsLatestBalance_ForEachBalanceType()
        {
            var transaction1 = FinancialTransaction.Create(
            TransactionSideType.Credit,
            BalanceType.Effective,
            ActivityType.DealerTrade,
            new Money(100m),
            new FinancialBalance(200m),
            TradeId.New());

            var transaction3 = FinancialTransaction.Create(
            TransactionSideType.Credit,
            BalanceType.Effective,
            ActivityType.DealerTrade,
            new Money(100m),
            new FinancialBalance(100m),
            TradeId.New());

            var transaction2 = FinancialTransaction.Create(
            TransactionSideType.Credit,
            BalanceType.Actual,
            ActivityType.DealerTrade,
            new Money(100m),
            new FinancialBalance(500m),
            TradeId.New());

            var transaction4 = FinancialTransaction.Create(
            TransactionSideType.Credit,
            BalanceType.Actual,
            ActivityType.DealerTrade,
            new Money(100m),
            new FinancialBalance(300m),
            TradeId.New());


            _transactionRepositoryMock.Setup(r => r.StartQuery(true, true)).Returns(new[] { transaction1, transaction2, transaction3, transaction4 }.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(new GetCurrentBalanceQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(400, result.Actual);
            Assert.Equal(200, result.AvailableForTrading);
        }
    }
}
