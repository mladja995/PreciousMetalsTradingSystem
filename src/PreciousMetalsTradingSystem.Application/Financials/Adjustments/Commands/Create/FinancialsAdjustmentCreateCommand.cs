using PreciousMetalsTradingSystem.Application.Common.Locking;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Financials.Adjustments.Commands.Create
{
    public class FinancialsAdjustmentCreateCommand : IRequest<Guid>, ILockable
    {
        public required DateTime Date { get; init; }
        public required TransactionSideType SideType { get; init; }
        public required decimal Amount { get; init; }
        public string? Note { get; init; }

        public string GetLockKey()
            => CommonLockKeyType.FinancialsAndOrPositionsAffectedLockKey.ToString();
    }
}
