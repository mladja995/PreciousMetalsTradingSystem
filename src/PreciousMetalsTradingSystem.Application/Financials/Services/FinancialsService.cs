using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.DomainServices;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Financials.Services
{
    /// <summary>
    /// Provides financial services for managing transactions and balances.
    /// This service maintains an in-memory cache of the latest balances for performance
    /// and consistency when creating multiple transactions within a single request scope.
    /// </summary>
    public class FinancialsService : IFinancialsService
    {
        private readonly IRepository<FinancialTransaction, FinancialTransactionId> _repository;
        private readonly IFinancialBalanceValidator _balanceValidator;

        /// <summary>
        /// In-memory cache of the most recent balance per balance type.
        /// This ensures consistency when creating multiple transactions in sequence
        /// before they are persisted to the database.
        /// </summary>
        private readonly ConcurrentDictionary<BalanceType, FinancialBalance> _balanceCache = new();

        /// <summary>
        /// In-memory tracking of created transactions that haven't been persisted yet.
        /// </summary>
        private readonly ConcurrentDictionary<BalanceType, List<FinancialTransaction>> _pendingTransactions = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialsService"/> class.
        /// </summary>
        /// <param name="repository">The repository for financial transactions.</param>
        /// <param name="balanceValidator">The validator for financial balances.</param>
        public FinancialsService(
            IRepository<FinancialTransaction, FinancialTransactionId> repository,
            IFinancialBalanceValidator balanceValidator)
        {
            _repository = repository.ThrowIfNull().Value;
            _balanceValidator = balanceValidator.ThrowIfNull().Value;
        }

        /// <inheritdoc/>
        public async Task<FinancialBalance> GetCurrentBalanceAsync(
            BalanceType balanceType,
            CancellationToken cancellationToken = default)
        {
            // If we already have this balance in the cache, return it
            if (_balanceCache.TryGetValue(balanceType, out var cachedBalance))
            {
                return cachedBalance;
            }

            // Otherwise, fetch from the database
            var latestBalance = await _repository.StartQuery(readOnly: true)
                .Where(x => x.BalanceType == balanceType)
                .OrderByDescending(x => x.TimestampUtc)
                .Select(x => x.Balance)
                .FirstOrDefaultAsync(cancellationToken);

            var currentBalance = latestBalance is null
                ? new FinancialBalance(0m)
                : latestBalance;

            // Cache the result
            _balanceCache[balanceType] = currentBalance;

            return currentBalance;
        }

        /// <inheritdoc/>
        public async Task<FinancialTransaction> CreateFinancialTransactionAsync(
            ActivityType activityType,
            TransactionSideType sideType,
            BalanceType balanceType,
            Money amount,
            IEntityId relatedActivity, 
            CancellationToken cancellationToken = default)
        {
            amount.ThrowIfNull();
            relatedActivity.ThrowIfNull(); 

            // Get the current balance
            var currentBalance = await GetCurrentBalanceAsync(balanceType, cancellationToken);

            // Create the transaction directly using the entity's Create method
            var newTransaction = FinancialTransaction.Create(
                sideType,
                balanceType,
                activityType,
                amount,
                currentBalance,
                relatedActivity);

            // Update the balance cache with the new balance
            _balanceCache[balanceType] = newTransaction.Balance;

            // Track this transaction as pending for this balance type
            if (!_pendingTransactions.TryGetValue(balanceType, out var transactions))
            {
                transactions = [];
                _pendingTransactions[balanceType] = transactions;
            }
            transactions.Add(newTransaction);

            return newTransaction;
        }

        /// <inheritdoc/>
        public async Task<bool> HasEnoughCashForBuyAsync(
            BalanceType balanceType,
            Money amount,
            CancellationToken cancellationToken = default)
        {
            amount.ThrowIfNull();

            var currentBalance = await GetCurrentBalanceAsync(balanceType, cancellationToken);

            // Use the validator directly
            return _balanceValidator.IsSufficientForDebit(amount, currentBalance);
        }
    }
}
