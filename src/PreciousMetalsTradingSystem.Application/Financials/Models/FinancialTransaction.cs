using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Financials.Models
{
    public class FinancialTransaction
    {
        public required Guid Id { get; init; }
        public required DateTime DateUtc { get; init; }
        public required BalanceType BalanceType { get; init; }
        public required ActivityType ActivityType { get; init; }
        public required string ActivityReference { get; init; }
        public required TransactionSideType SideType { get; init; }
        public required bool IsActivityFinancialSettled { get; init; }
        public required decimal Amount { get; init; }
        public required decimal Balance { get; init; }
        public required string? Note { get; init; }

        public static readonly Func<Domain.Entities.FinancialTransaction, FinancialTransaction> Projection =
            entity => new FinancialTransaction
            {
                Id = entity.Id,
                DateUtc = entity.TimestampUtc,
                BalanceType = entity.BalanceType,
                ActivityType = entity.ActivityType,
                ActivityReference = entity.Trade?.TradeNumber ?? string.Empty,
                SideType = entity.SideType,
                IsActivityFinancialSettled = entity.Trade?.IsFinancialSettled ?? true,
                Amount = entity.Amount,
                Balance = entity.Balance,
                Note = entity.FinancialAdjustment?.Note,
            };
    }
}
