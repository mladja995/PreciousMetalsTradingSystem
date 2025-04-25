using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class ProductLocationPosition : AggregateRoot<ProductLocationPositionId>
    {
        public ProductId ProductId { get; private set; }
        public TradeId TradeId { get; private set; }
        public LocationType LocationType { get; private set; }
        public DateTime TimestampUtc { get; private set; }
        public PositionSideType SideType { get; private set; }
        public PositionType Type { get; private set; }
        public QuantityUnits QuantityUnits { get; private set; }
        public PositionQuantityUnits PositionUnits {  get; private set; } // Represents running position for product on location

        public virtual Product Product { get; private set; }
        public virtual Trade Trade { get; private set; }

        
        public static ProductLocationPosition Create(
            ProductId productId,
            TradeId relatedTradeId,
            LocationType location,
            PositionSideType sideType,
            PositionType type,
            QuantityUnits quantityUnits,
            PositionQuantityUnits currentPositionQuantityUnits)
        {
            productId.ThrowIfNull();
            relatedTradeId.ThrowIfNull();
            quantityUnits.ThrowIfNull();
            currentPositionQuantityUnits.ThrowIfNull();

            // IMPORTANT: Below creation of position can lead to negative running balance.
            // Operations team agreed for this potential behaviour.
            PositionQuantityUnits positionQuantityUnits = new(currentPositionQuantityUnits + (int)sideType * quantityUnits);

            var entity = new ProductLocationPosition
            {
                Id = ProductLocationPositionId.New(),
                ProductId = productId,
                TradeId = relatedTradeId,
                LocationType = location,
                SideType = sideType,
                Type = type,
                TimestampUtc = DateTime.UtcNow,
                QuantityUnits = quantityUnits,
                PositionUnits = positionQuantityUnits
            };

            // Create and add the domain event
            entity.AddDomainEvent(ProductLocationPositionCreatedEvent.FromEntity(entity));

            return entity;
        }
    }
}
