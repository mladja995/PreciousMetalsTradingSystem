using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Domain.DomainServices
{
    /// <summary>
    /// Defines validation rules for financial balances.
    /// </summary>
    public interface IFinancialBalanceValidator
    {
        /// <summary>
        /// Determines if there is sufficient balance for a debit operation.
        /// </summary>
        /// <param name="debitAmount">The amount to be debited.</param>
        /// <param name="currentBalance">The current balance to evaluate.</param>
        /// <returns>True if the balance is sufficient; otherwise, false.</returns>
        bool IsSufficientForDebit(
            Money debitAmount,
            FinancialBalance currentBalance);
    }
}
