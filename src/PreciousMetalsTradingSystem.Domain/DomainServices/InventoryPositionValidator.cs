using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.DomainServices
{
    /// <summary>
    /// Provides validation rules for inventory positions.
    /// </summary>
    public class InventoryPositionValidator : IInventoryPositionValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryPositionValidator"/> class.
        /// </summary>
        public InventoryPositionValidator() { }

        /// <inheritdoc/>
        public bool IsSufficientForSell(
            QuantityUnits requestedQuantity,
            PositionQuantityUnits currentPosition)
        {
            requestedQuantity.ThrowIfNull();
            currentPosition.ThrowIfNull();

            return (currentPosition - requestedQuantity.Value) >= 0;
        }
    }
}
