using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Application.Financials.Services
{
    /// <summary>
    /// Defines a provider that supplies the current financial balance for a given balance type.
    /// This interface is used to obtain current balances for financial operations.
    /// </summary>
    public interface IFinancialBalanceProvider
    {
        /// <summary>
        /// Gets the current balance for the specified balance type.
        /// </summary>
        /// <param name="balanceType">The type of balance to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The current financial balance.</returns>
        Task<FinancialBalance> GetCurrentBalanceAsync(
            BalanceType balanceType,
            CancellationToken cancellationToken = default);
    }
}
