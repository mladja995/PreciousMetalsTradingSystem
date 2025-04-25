using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Update
{
    public class HedgingItemUpdateCommand : IRequest
    {
        [OpenApiExclude]
        public Guid AccountId { get; set; }
        [OpenApiExclude]
        public Guid HedgingItemId { get; set; }
        public required DateTime Date { get; init; }
        public required HedgingItemType HedgingItemType { get; init; }
        public required HedgingItemSideType SideType { get; init; }
        public required decimal Amount { get; init; }
        public string? Note { get; init; }
    }
}
