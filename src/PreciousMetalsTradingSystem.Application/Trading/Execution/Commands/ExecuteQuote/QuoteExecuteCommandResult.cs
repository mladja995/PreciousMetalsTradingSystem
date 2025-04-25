using PreciousMetalsTradingSystem.Application.Trading.Execution.Models;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.ExecuteQuote
{
    public class QuoteExecuteCommandResult
    {
        public required Guid TradeId { get; init; }
        public required string TradeNumber { get; init; }
        public required DateTime ExecutedOnUtc { get; init; }
        public required Quote Quote { get; init; }
    }
}
