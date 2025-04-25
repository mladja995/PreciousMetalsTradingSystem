using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Event raised when a Trade is financially settled
    /// </summary>
    public record TradeFinancialSettledEvent(
        TradeId Id,
        string TradeNumber,
        DateTime SettledOnUtc) : DomainEvent(nameof(Trade))
    {
        /// <summary>
        /// Creates a TradeFinancialSettledEvent from a Trade entity
        /// </summary>
        /// <param name="trade">The trade that was financially settled</param>
        /// <returns>A new TradeFinancialSettledEvent</returns>
        public static TradeFinancialSettledEvent FromEntity(Trade trade)
        {
            return new TradeFinancialSettledEvent(
                trade.Id,
                trade.TradeNumber,
                trade.FinancialSettledOnUtc ?? DateTime.UtcNow);
        }
    }
}
