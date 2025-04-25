using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Domain.DomainServices
{
    /// <summary>
    /// Defines validation rules for inventory positions.
    /// </summary>
    public interface IInventoryPositionValidator
    {
        /// <summary>
        /// Determines if there is sufficient position quantity for a sell operation.
        /// </summary>
        /// <param name="requestedQuantity">The quantity requested for the sell operation.</param>
        /// <param name="currentPosition">The current position quantity to evaluate.</param>
        /// <returns>True if the position is sufficient; otherwise, false.</returns>
        bool IsSufficientForSell(
            QuantityUnits requestedQuantity,
            PositionQuantityUnits currentPosition);
    }
}
