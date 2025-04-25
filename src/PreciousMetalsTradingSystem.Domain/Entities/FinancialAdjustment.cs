using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class FinancialAdjustment : AggregateRoot<FinancialAdjustmentId>
    {
        public DateOnly AdjustmentDate { get; private set; }
        public DateTime TimestampUtc { get; private set; }
        public TransactionSideType SideType { get; private set; }
        public Money Amount { get; private set; }
        public string? Note { get; private set; }

        public virtual ICollection<FinancialTransaction> FinancialTransactions { get; } = [];

        public static FinancialAdjustment Create(
            DateOnly date,
            TransactionSideType sideType,
            Money amount,
            string? note)
        {
            return new FinancialAdjustment
            {
                Id = FinancialAdjustmentId.New(),
                AdjustmentDate = date,
                TimestampUtc = DateTime.UtcNow,
                SideType = sideType,
                Amount = amount,
                Note = note,
            };

        }

        public void AddFinancialTransaction(FinancialTransaction transaction)
        {
            EnsureOnlyOneTransactionPerBalanceType(transaction.BalanceType);
            FinancialTransactions.Add(transaction);
        }

        private void EnsureOnlyOneTransactionPerBalanceType(BalanceType balanceType)
        {
            if (FinancialTransactions.Any(x => x.BalanceType.Equals(balanceType)))
            {
                throw new DuplicatedFinancialTransactionPerBalanceType(balanceType);
            }
        }
    }
}
