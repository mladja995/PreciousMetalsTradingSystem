using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Activity.Models
{
    public class ActivityItemResult
    {
        public required Guid Id { get; init; }
        public required DateOnly TradeDate { get; init; }
        public required DateTime TimestampUtc { get; init; }
        public required string TradeNumber { get; init; }
        public required TradeType Type { get; init; }
        public required SideType SideType { get; init; }
        public required LocationType Location { get; init; }
        public required DateTime LastUpdatedOnUtc {  get; init; }

        public required string ProductSKU { get; init; }
        public required string ProductName { get; init; }
        public required bool IsPositionSettled { get; init; }
        public required bool IsFinancialSettled { get; init; }
        public required DateOnly FinancialSettledOn { get; init; }
        public required int UnitQuantity { get; init; }
        public required decimal Amount { get; init; }
        public required decimal PricePerUnit { get; init; }
    }
}
