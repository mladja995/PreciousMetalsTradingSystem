using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Event raised when a Trade is confirmed
    /// </summary>
    public record TradeConfirmedEvent(
        TradeId Id,
        string TradeNumber,
        DateTime ConfirmedOnUtc) : DomainEvent(nameof(Trade))
    {
        /// <summary>
        /// Creates a TradeConfirmedEvent from a Trade entity
        /// </summary>
        /// <param name="trade">The trade that was confirmed</param>
        /// <returns>A new TradeConfirmedEvent</returns>
        public static TradeConfirmedEvent FromEntity(Trade trade)
        {
            return new TradeConfirmedEvent(
                trade.Id,
                trade.TradeNumber,
                trade.ConfirmedOnUtc ?? DateTime.UtcNow);
        }
    }
}
