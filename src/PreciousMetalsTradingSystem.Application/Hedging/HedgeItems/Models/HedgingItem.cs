using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Models
{
    public class HedgingItem
    {
        public required Guid Id { get; init; }
        public required DateOnly Date { get; init; }
        public required HedgingItemType Type { get; init; }
        public required HedgingItemSideType SideType { get; init; }
        public required decimal Amount { get; init; }
        public string? Note { get; init; }

        public static readonly Func<Domain.Entities.HedgingItem, HedgingItem> Projection =
            entity => new HedgingItem
            {
                Id = entity.Id,
                Date = entity.HedgingItemDate,
                Type = entity.Type,
                SideType = entity.SideType,
                Amount = entity.Amount,
                Note = entity.Note
            };
    }
}
