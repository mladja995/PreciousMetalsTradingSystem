using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Event raised when a Trade's positions are settled
    /// </summary>
    public record TradePositionsSettledEvent(
        TradeId Id,
        string TradeNumber,
        DateTime SettledOnUtc) : DomainEvent(nameof(Trade))
    {
        /// <summary>
        /// Creates a TradePositionsSettledEvent from a Trade entity
        /// </summary>
        /// <param name="trade">The trade that was settled</param>
        /// <returns>A new TradePositionsSettledEvent</returns>
        public static TradePositionsSettledEvent FromEntity(Trade trade)
        {
            return new TradePositionsSettledEvent(
                trade.Id,
                trade.TradeNumber,
                trade.PositionSettledOnUtc ?? DateTime.UtcNow);
        }
    }
}
