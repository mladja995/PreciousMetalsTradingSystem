using Moq;
using PreciousMetalsTradingSystem.Application.Financials.Queries.GetTransactions;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using MockQueryable;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Financial.Handlers.Queries
{
    public class GetTransactionsQueryTests
    {
        private readonly Mock<IRepository<FinancialTransaction, FinancialTransactionId>> _repositoryMock;
        private readonly GetTransactionsQueryHandler _handler;

        public GetTransactionsQueryTests()
        {
            _repositoryMock = new Mock<IRepository<FinancialTransaction, FinancialTransactionId>>();
            _handler = new GetTransactionsQueryHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenDataExists()
        {
            // Arrange
            var request = new GetTransactionsQuery
            {
                BalanceType = BalanceType.Effective,
                PageNumber = 1,
                PageSize = 10,
                FromDate = DateTime.MinValue.ConvertUtcToEst(),
                ToDate = DateTime.MaxValue.ConvertUtcToEst()
            };
            var trade = Trade.Create(
                TradeType.ClientTrade, 
                SideType.Buy, 
                LocationType.SLC, 
                DateOnly.MaxValue, 
                DateOnly.MaxValue, 
                "Test");

            var transaction = FinancialTransaction.Create(
                TransactionSideType.Debit, 
                BalanceType.Effective, 
                ActivityType.ClientTrade, 
                new Money(22), 
                new FinancialBalance(33),
                trade.Id);
            
            trade.AddFinancialTransaction(transaction);

            var mockData = new List<FinancialTransaction> { transaction };
            var mockQueryable = mockData.AsQueryable().BuildMock();

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockQueryable);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyResult_WhenNoMatchingData()
        {
            // Arrange
            var request = new GetTransactionsQuery
            {
                BalanceType = BalanceType.Actual,
                PageNumber = 1,
                PageSize = 10,
                FromDate = DateTime.MinValue.ConvertUtcToEst(),
                ToDate = DateTime.MaxValue.ConvertUtcToEst()
            };
            
            var trade = Trade.Create(
                TradeType.ClientTrade, 
                SideType.Buy, 
                LocationType.SLC, 
                DateOnly.MaxValue, 
                DateOnly.MaxValue, 
                "Test");
            
            var transaction = FinancialTransaction.Create(
                TransactionSideType.Debit,
                BalanceType.Effective, 
                ActivityType.ClientTrade, 
                new Money(22), 
                new FinancialBalance(33),
                trade.Id);

            trade.AddFinancialTransaction(transaction);
            DateTime? fromDateUtc = request.FromDate.HasValue ? request.FromDate.Value.Date.ConvertEstToUtc() : null;
            DateTime? toDateUtc = request.ToDate.HasValue ? request.ToDate.Value.Date.ConvertEstToUtc() : null;

            var mockData = new List<FinancialTransaction> { transaction };
            var mockQueryable = mockData.AsQueryable().BuildMock();

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockQueryable);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
        }
    }
}