using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class FinancialTransaction : AggregateRoot<FinancialTransactionId>
    {
        public TradeId? TradeId { get; private set; }
        public FinancialAdjustmentId? FinancialAdjustmentId { get; private set; }
        public DateTime TimestampUtc { get; private set; }
        public BalanceType BalanceType { get; private set; }
        public TransactionSideType SideType { get; private set; }
        public ActivityType ActivityType { get; private set; }
        public Money Amount { get; private set; } 
        public FinancialBalance Balance { get; private set; }
        public virtual Trade Trade { get; private set; }
        public virtual FinancialAdjustment FinancialAdjustment { get; private set; }

        public static FinancialTransaction Create(
            TransactionSideType sideType, 
            BalanceType balanceType, 
            ActivityType activityType,
            Money amount,
            FinancialBalance currentBalance,
            IEntityId relatedActivity) 
        {
            amount.ThrowIfNull();
            currentBalance.ThrowIfNull();

            var balanceAfter = CalculateBalanceAfter(currentBalance, sideType, amount);
            EnsureValidBalanceAfter(balanceAfter);

            var entity = new FinancialTransaction
            {
                Id = FinancialTransactionId.New(),
                TimestampUtc = DateTime.UtcNow,
                BalanceType = balanceType,
                SideType = sideType,
                ActivityType = activityType,
                Amount = amount,
                Balance = balanceAfter
            }.SetRelatedActivity(relatedActivity);

            // Create and add the domain event
            entity.AddDomainEvent(FinancialTransactionCreatedEvent.FromEntity(entity));

            return entity;
        }

        private static void EnsureValidBalanceAfter(FinancialBalance balanceAfter)
        {
            balanceAfter
                .Value
                .Throw(() => new FinancialTransacitionNegativeBalanceException())
                .IfLessThan(0m);
        }

        private static FinancialBalance CalculateBalanceAfter(
            FinancialBalance currentBalance,
            TransactionSideType side,
            Money amount) => new(currentBalance + (int)side * amount);

        private FinancialTransaction SetRelatedActivity( 
            IEntityId relatedActivityId)
        {
            if (ActivityType is ActivityType.OffsetTrade
                or ActivityType.ClientTrade
                or ActivityType.DealerTrade)
            {
                TradeId = (TradeId)relatedActivityId;
                TradeId.ThrowIfNull();
                return this;
            }

            if (ActivityType is ActivityType.Adjustment)
            {
                FinancialAdjustmentId = (FinancialAdjustmentId)relatedActivityId;
                FinancialAdjustmentId.ThrowIfNull();
                return this;
            }

            throw new NotSupportedRelatedActivityTypeException(ActivityType);
        }
    }
}
