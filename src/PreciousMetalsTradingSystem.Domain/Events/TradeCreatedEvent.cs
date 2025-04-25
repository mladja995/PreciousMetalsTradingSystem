using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Domain event that is raised when a new trade is created
    /// </summary>
    public record TradeCreatedEvent(TradeId TradeId,
                TradeType TradeType,
                SideType SideType,
                LocationType LocationType,
                DateOnly TradeDate,
                DateOnly FinancialsSettleOn,
                DateTime TimestampUtc,
                string? Note,
                string TradeNumber) : DomainEvent(nameof(Trade))
    {
        /// <summary>
        /// Creates a Trade
        /// </summary>
        /// <returns>A new TradeCreatedEvent</returns>
        public static TradeCreatedEvent FromEntity(Trade trade)
        {
            return new TradeCreatedEvent(
                trade.Id,
                trade.Type,
                trade.Side,
                trade.LocationType,
                trade.TradeDate,
                trade.FinancialSettleOn,
                trade.TimestampUtc,
                trade.Note,
                trade.TradeNumber
              );
        }
    }
}
