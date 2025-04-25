using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Event raised when a new FinancialTransaction is created
    /// </summary>
    public record FinancialTransactionCreatedEvent(
        FinancialTransactionId TransactionId,
        DateTime TimestampUtc,
        BalanceType BalanceType,
        TransactionSideType SideType,
        ActivityType ActivityType,
        Money Amount,
        FinancialBalance Balance) : DomainEvent(nameof(FinancialTransaction))
    {
        /// <summary>
        /// Creates a FinancialTransactionCreatedEvent from a FinancialTransaction entity
        /// </summary>
        /// <param name="transaction">The financial transaction that was created</param>
        /// <returns>A new FinancialTransactionCreatedEvent</returns>
        public static FinancialTransactionCreatedEvent FromEntity(FinancialTransaction transaction)
        {
            return new FinancialTransactionCreatedEvent(
                transaction.Id,
                transaction.TimestampUtc,
                transaction.BalanceType,
                transaction.SideType,
                transaction.ActivityType,
                transaction.Amount,
                transaction.Balance);
        }
    }
}
