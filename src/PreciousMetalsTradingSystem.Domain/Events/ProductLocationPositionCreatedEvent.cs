using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Event raised when a new ProductLocationPosition is created
    /// </summary>
    public record ProductLocationPositionCreatedEvent(
        ProductLocationPositionId PositionId,
        ProductId ProductId,
        LocationType LocationType,
        DateTime TimestampUtc,
        PositionSideType SideType,
        PositionType Type,
        QuantityUnits QuantityUnits,
        PositionQuantityUnits PositionUnits) : DomainEvent(nameof(ProductLocationPosition))
    {
        /// <summary>
        /// Creates a ProductLocationPositionCreatedEvent from a ProductLocationPosition entity
        /// </summary>
        /// <param name="position">The position that was created</param>
        /// <returns>A new ProductLocationPositionCreatedEvent</returns>
        public static ProductLocationPositionCreatedEvent FromEntity(ProductLocationPosition position)
        {
            return new ProductLocationPositionCreatedEvent(
                position.Id,
                position.ProductId,
                position.LocationType,
                position.TimestampUtc,
                position.SideType,
                position.Type,
                position.QuantityUnits,
                position.PositionUnits);
        }
    }
}
