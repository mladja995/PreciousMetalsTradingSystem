using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Models
{
    public class Quote
    {
        public required Guid Id { get; init; }
        public required DateTime IssuedAtUtc { get; init; }
        public required DateTime ExpiriesAtUtc { get; init; }
        public required ClientSideType Side { get; init; }
        public required LocationType Location { get; init; }
        public required IEnumerable<QuoteItem> Items { get; init; }
    }
}
