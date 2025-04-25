using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.DomainServices
{
    /// <summary>
    /// Provides validation rules for financial balances.
    /// </summary>
    public class FinancialBalanceValidator : IFinancialBalanceValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialBalanceValidator"/> class.
        /// </summary>
        public FinancialBalanceValidator() { }

        /// <inheritdoc/>
        public bool IsSufficientForDebit(
            Money debitAmount,
            FinancialBalance currentBalance)
        {
            debitAmount.ThrowIfNull();
            currentBalance.ThrowIfNull();

            return currentBalance - debitAmount >= 0m;
        }
    }
}
