using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using System.Linq.Expressions;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Inventory.Models
{
    /// <summary>
    /// Represents the historical position of a product at a specific location and time, 
    /// including trade details like type, side (buy/sell), position state, and quantity.
    /// </summary>
    public class ProductLocationPositionHistory
    {
        public required Guid Id { get; init; }
        public required DateTime TimestampUtc { get; init; }
        public required string ProductSKU { get; init; }
        public required string ProductName { get; init; }
        public required string TradeReference { get; init; }
        public required TradeType TradeType { get; init; }
        public required LocationType Location { get; init; }
        public required PositionSideType PositionSideType { get; init; }
        public required PositionType PositionType { get; init; }
        public required int QuantityUnits { get; init; }
        public required int PositionUnits { get; init; }

        public static readonly Expression<Func<ProductLocationPosition, ProductLocationPositionHistory>> Projection =
            position => new ProductLocationPositionHistory
            {
                Id = position.Id.Value,
                TimestampUtc = position.TimestampUtc,
                ProductSKU = position.Product.SKU,
                ProductName = position.Product.Name,
                TradeReference = position.Trade.TradeNumber,
                TradeType = position.Trade.Type,
                Location = position.LocationType,
                PositionSideType = position.SideType,
                PositionType = position.Type,
                QuantityUnits = position.QuantityUnits,
                PositionUnits = position.PositionUnits
            };
    }
}
