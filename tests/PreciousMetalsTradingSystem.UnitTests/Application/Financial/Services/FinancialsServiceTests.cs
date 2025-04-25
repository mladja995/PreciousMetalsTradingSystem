using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Domain.DomainServices;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MockQueryable;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Financial.Services
{
    public class FinancialsServiceTests
    {
        private readonly Mock<IRepository<FinancialTransaction, FinancialTransactionId>> _repositoryMock;
        private readonly Mock<IFinancialBalanceValidator> _validatorMock;
        private readonly FinancialsService _service;

        public FinancialsServiceTests()
        {
            _repositoryMock = new Mock<IRepository<FinancialTransaction, FinancialTransactionId>>();
            _validatorMock = new Mock<IFinancialBalanceValidator>();

            _service = new FinancialsService(_repositoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task GetCurrentBalanceAsync_WhenCacheIsEmpty_FetchesFromDatabase()
        {
            // Arrange
            var balanceType = BalanceType.Effective;
            var initialBalance = new FinancialBalance(0m);
            var amount = new Money(1000m);

            var transaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                balanceType,
                ActivityType.ClientTrade,
                amount,
                initialBalance,
                TradeId.New());

            SetupRepository(transaction);

            // Act
            var result = await _service.GetCurrentBalanceAsync(balanceType);

            // Assert
            Assert.Equal(transaction.Balance, result);
            _repositoryMock.Verify(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GetCurrentBalanceAsync_WhenCacheHasEntry_ReturnsFromCache()
        {
            // Arrange
            var balanceType = BalanceType.Effective;
            var initialBalance = new FinancialBalance(0m);
            var amount = new Money(1000m);

            var transaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                balanceType,
                ActivityType.ClientTrade,
                amount,
                initialBalance,
                TradeId.New());

            SetupRepository(transaction);

            // First call to populate cache
            await _service.GetCurrentBalanceAsync(balanceType);

            // Reset mock to verify it's not called again
            _repositoryMock.Reset();
            SetupRepository(transaction);

            // Act
            var result = await _service.GetCurrentBalanceAsync(balanceType);

            // Assert
            Assert.Equal(transaction.Balance, result);
            _repositoryMock.Verify(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task GetCurrentBalanceAsync_WhenNoTransactionsExist_ReturnsZeroBalance()
        {
            // Arrange
            var balanceType = BalanceType.Effective;
            SetupEmptyRepository();

            // Act
            var result = await _service.GetCurrentBalanceAsync(balanceType);

            // Assert
            Assert.Equal(new FinancialBalance(0m), result);
        }

        [Fact]
        public async Task CreateFinancialTransactionAsync_UpdatesBalanceCache()
        {
            // Arrange
            var balanceType = BalanceType.Effective;
            var initialBalance = new FinancialBalance(1000m);
            var amount = new Money(200m);

            var lastTransaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                balanceType,
                ActivityType.ClientTrade,
                new Money(initialBalance.Value),
                new FinancialBalance(0m),
                TradeId.New());

            SetupRepository(lastTransaction);

            // Act
            var result = await _service.CreateFinancialTransactionAsync(
                ActivityType.ClientTrade,
                TransactionSideType.Credit,
                balanceType,
                amount,
                TradeId.New());

            // Assert
            // Get balance from cache to verify it was updated
            var updatedBalance = await _service.GetCurrentBalanceAsync(balanceType);
            Assert.Equal(result.Balance, updatedBalance);
            Assert.NotEqual(initialBalance, updatedBalance);
        }

        [Fact]
        public async Task HasEnoughCashForBuyAsync_CallsValidator()
        {
            // Arrange
            var balanceType = BalanceType.Effective;
            var initialBalance = new FinancialBalance(0m);
            var currentBalance = new FinancialBalance(1000m);
            var amount = new Money(500m);

            var transaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                balanceType,
                ActivityType.ClientTrade,
                new Money(1000m),
                initialBalance,
                TradeId.New());

            SetupRepository(transaction);
            _validatorMock.Setup(v => v.IsSufficientForDebit(amount, currentBalance))
                .Returns(true);

            // Act
            var result = await _service.HasEnoughCashForBuyAsync(balanceType, amount);

            // Assert
            Assert.True(result);
            _validatorMock.Verify(v => v.IsSufficientForDebit(amount, currentBalance), Times.Once);
        }

        [Fact]
        public async Task CreateFinancialTransactionAsync_MultipleTransactions_UsesCorrectBalances()
        {
            // Arrange
            var balanceType = BalanceType.Effective;
            var initialBalance = new FinancialBalance(1000m);
            var amount1 = new Money(200m);
            var amount2 = new Money(300m);
            var amount3 = new Money(150m);

            var lastTransaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                balanceType,
                ActivityType.ClientTrade,
                new Money(initialBalance.Value),
                new FinancialBalance(0m),
                TradeId.New());

            SetupRepository(lastTransaction);

            // Act - Create multiple transactions
            var transaction1 = await _service.CreateFinancialTransactionAsync(
                ActivityType.ClientTrade,
                TransactionSideType.Debit,
                balanceType,
                amount1,
                TradeId.New());

            var transaction2 = await _service.CreateFinancialTransactionAsync(
                ActivityType.ClientTrade,
                TransactionSideType.Debit,
                balanceType,
                amount2,
                TradeId.New());

            var transaction3 = await _service.CreateFinancialTransactionAsync(
                ActivityType.ClientTrade,
                TransactionSideType.Debit,
                balanceType,
                amount3,
                TradeId.New());

            // Assert
            // Calculate expected balances
            var expectedBalance1 = new FinancialBalance(initialBalance.Value - amount1.Value);
            var expectedBalance2 = new FinancialBalance(expectedBalance1.Value - amount2.Value);
            var expectedBalance3 = new FinancialBalance(expectedBalance2.Value - amount3.Value);

            // First transaction should use initial balance
            Assert.Equal(expectedBalance1, transaction1.Balance);

            // Second transaction should use balance after first transaction
            Assert.Equal(expectedBalance2, transaction2.Balance);

            // Third transaction should use balance after second transaction
            Assert.Equal(expectedBalance3, transaction3.Balance);

            // Verify repository was called only once to get the initial balance
            _repositoryMock.Verify(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task CreateFinancialTransactionAsync_MultipleBalanceTypes_TracksEachTypeCorrectly()
        {
            // Arrange
            var effectiveType = BalanceType.Effective;
            var availableType = BalanceType.Actual;

            var initialEffectiveBalance = new FinancialBalance(1000m);
            var initialAvailableBalance = new FinancialBalance(500m);

            var effectiveAmount = new Money(200m);
            var availableAmount = new Money(100m);

            var effectiveTransaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                effectiveType,
                ActivityType.ClientTrade,
                new Money(initialEffectiveBalance.Value),
                new FinancialBalance(0m),
                TradeId.New());

            var availableTransaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                availableType,
                ActivityType.ClientTrade,
                new Money(initialAvailableBalance.Value),
                new FinancialBalance(0m),
                TradeId.New());

            SetupRepositoryWithMultipleTypes(effectiveTransaction, availableTransaction);

            // Act
            var effectiveTx1 = await _service.CreateFinancialTransactionAsync(
                ActivityType.ClientTrade,
                TransactionSideType.Debit,
                effectiveType,
                effectiveAmount,
                TradeId.New());

            var availableTx1 = await _service.CreateFinancialTransactionAsync(
                ActivityType.ClientTrade,
                TransactionSideType.Debit,
                availableType,
                availableAmount,
                TradeId.New());

            var effectiveTx2 = await _service.CreateFinancialTransactionAsync(
                ActivityType.ClientTrade,
                TransactionSideType.Debit,
                effectiveType,
                effectiveAmount,
                TradeId.New());

            // Assert
            // Calculate expected balances
            var expectedEffectiveBalance1 = new FinancialBalance(initialEffectiveBalance.Value - effectiveAmount.Value);
            var expectedAvailableBalance1 = new FinancialBalance(initialAvailableBalance.Value - availableAmount.Value);
            var expectedEffectiveBalance2 = new FinancialBalance(expectedEffectiveBalance1.Value - effectiveAmount.Value);

            // Effective balance should be tracked separately from available balance
            Assert.Equal(expectedEffectiveBalance1, effectiveTx1.Balance);
            Assert.Equal(expectedAvailableBalance1, availableTx1.Balance);
            Assert.Equal(expectedEffectiveBalance2, effectiveTx2.Balance);

            // Final balances should be correct
            var finalEffectiveBalance = await _service.GetCurrentBalanceAsync(effectiveType);
            var finalAvailableBalance = await _service.GetCurrentBalanceAsync(availableType);

            Assert.Equal(expectedEffectiveBalance2, finalEffectiveBalance);
            Assert.Equal(expectedAvailableBalance1, finalAvailableBalance);
        }

        [Theory]
        [InlineData(TransactionSideType.Credit, 1000, 200, 1200)]
        [InlineData(TransactionSideType.Debit, 1000, 200, 800)]
        [InlineData(TransactionSideType.Credit, 0, 500, 500)]
        [InlineData(TransactionSideType.Debit, 500, 500, 0)]
        public async Task CreateFinancialTransactionAsync_WithDifferentSideTypes_CalculatesBalanceCorrectly(
            TransactionSideType sideType, decimal initialBalanceAmount, decimal transactionAmount, decimal expectedBalanceAmount)
        {
            // Arrange
            var balanceType = BalanceType.Effective;
            var initialBalance = new FinancialBalance(initialBalanceAmount);
            var amount = new Money(transactionAmount);

            var lastTransaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                balanceType,
                ActivityType.ClientTrade,
                new Money(initialBalance.Value),
                new FinancialBalance(0m),
                TradeId.New());

            SetupRepository(lastTransaction);

            // Act
            var transaction = await _service.CreateFinancialTransactionAsync(
                ActivityType.ClientTrade,
                sideType,
                balanceType,
                amount,
                TradeId.New());

            // Assert
            Assert.Equal(new FinancialBalance(expectedBalanceAmount), transaction.Balance);

            // Verify cache was updated
            var cachedBalance = await _service.GetCurrentBalanceAsync(balanceType);
            Assert.Equal(new FinancialBalance(expectedBalanceAmount), cachedBalance);
        }

        private void SetupRepository(FinancialTransaction transaction)
        {
            var queryable = new List<FinancialTransaction> { transaction }.AsQueryable().BuildMock();

            _repositoryMock.Setup(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(queryable);
        }

        private void SetupEmptyRepository()
        {
            var queryable = new List<FinancialTransaction>().AsQueryable().BuildMock();

            _repositoryMock.Setup(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(queryable);
        }

        private void SetupRepositoryWithMultipleTypes(FinancialTransaction effectiveTransaction, FinancialTransaction availableTransaction)
        {
            var transactions = new List<FinancialTransaction> { effectiveTransaction, availableTransaction };
            var queryable = transactions.AsQueryable().BuildMock();

            _repositoryMock.Setup(r => r.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(queryable);
        }
    }
}
