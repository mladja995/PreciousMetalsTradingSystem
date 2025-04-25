using PreciousMetalsTradingSystem.Domain.Enums;
using LinqKit;
using System.Linq.Expressions;
using DomainEntities = PreciousMetalsTradingSystem.Domain.Entities;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Models
{
    public class TradeDetailsResult
    {
        public required Guid Id { get; init; }
        public required DateOnly TradeDate { get; init; }
        public required string TradeNumber { get; init; }
        public required TradeType Type { get; init; }
        public required SideType SideType { get; init; }
        public required LocationType Location { get; init; }
        public required bool IsPositionSettled { get; init; }
        public DateTime? PositionSettledOnUtc { get; init; }
        public required bool IsFinancialSettled { get; init; }
        public DateTime? FinancialSettledOnUtc { get; init; }
        public DateTime? ConfirmedOnUtc { get; init; }
        public string? Note { get; init; }

        public required IEnumerable<TradeItemDetailsResult> Items { get; init; }


        public static readonly Expression<Func<DomainEntities.Trade, TradeDetailsResult>> Projection = 
            trade => new TradeDetailsResult
            {
                Id = trade.Id,
                TradeDate = trade.TradeDate,
                TradeNumber = trade.TradeNumber,
                Type = trade.Type,
                SideType = trade.Side,
                Location = trade.LocationType,
                IsPositionSettled = trade.IsPositionSettled,
                PositionSettledOnUtc = trade.PositionSettledOnUtc,
                IsFinancialSettled = trade.IsFinancialSettled,
                FinancialSettledOnUtc = trade.FinancialSettledOnUtc,
                ConfirmedOnUtc = trade.ConfirmedOnUtc,
                Note = trade.Note,
                Items = trade.Items.Select(x => TradeItemDetailsResult.Projection.Invoke(x))
            };
    }
}
