using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class DuplicatedFinancialTransactionPerBalanceType : DomainRuleViolationException
    {
        public DuplicatedFinancialTransactionPerBalanceType(BalanceType balanceType) 
            : base($"Transaction for Balance Type '{balanceType}' already exists.")
        {
        }
    }
}
