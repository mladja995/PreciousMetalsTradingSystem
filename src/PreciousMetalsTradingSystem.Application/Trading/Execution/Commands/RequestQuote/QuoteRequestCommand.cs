using PreciousMetalsTradingSystem.Application.Trading.Execution.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.RequestQuote
{
    public class QuoteRequestCommand : IRequest<Quote>
    {
        public required LocationType Location { get; init; }
        public required ClientSideType SideType { get; init; }
        public string? Note { get; init; } = null;

        public required IEnumerable<QuoteRequestItem> Items { get; init; }
    }
}
