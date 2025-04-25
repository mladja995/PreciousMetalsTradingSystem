using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;
using PreciousMetalsTradingSystem.Application.Common.Locking;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.ExecuteQuote
{
    public class QuoteExecuteCommand : IRequest<QuoteExecuteCommandResult>, ILockable
    {
        [OpenApiExclude]
        public Guid Id { get; set; }

        public string? ReferenceNumber { get; init; }

        public string GetLockKey()
            => CommonLockKeyType.FinancialsAndOrPositionsAffectedLockKey.ToString();
    }
}
