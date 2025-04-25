using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Application.Financials.Services
{
    /// <summary>
    /// Defines financial services for managing transactions and balances.
    /// </summary>
    public interface IFinancialsService : IFinancialBalanceProvider
    {
        /// <summary>
        /// Creates a new financial transaction with the specified details.
        /// All transactions created within the same service scope will use 
        /// the correct sequential balances, even if not yet persisted to the database.
        /// </summary>
        /// <param name="activityType">The type of activity for this transaction.</param>
        /// <param name="sideType">The side type of the transaction (debit/credit).</param>
        /// <param name="balanceType">The type of balance affected.</param>
        /// <param name="amount">The monetary amount of the transaction.</param>
        /// <param name="relatedActivity">The related activity entity ID.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The newly created financial transaction.</returns>
        Task<FinancialTransaction> CreateFinancialTransactionAsync(
            ActivityType activityType,
            TransactionSideType sideType,
            BalanceType balanceType,
            Money amount,
            IEntityId relatedActivity, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines if there is enough cash available for a purchase operation.
        /// </summary>
        /// <param name="balanceType">The type of balance to check.</param>
        /// <param name="amount">The amount required for the purchase.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if there is enough cash available; otherwise, false.</returns>
        Task<bool> HasEnoughCashForBuyAsync(
            BalanceType balanceType,
            Money amount,
            CancellationToken cancellationToken = default);
    }
}
