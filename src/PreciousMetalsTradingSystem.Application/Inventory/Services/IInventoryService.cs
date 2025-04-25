using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Application.Inventory.Services
{
    /// <summary>
    /// Defines inventory services for managing product positions and quantities.
    /// </summary>
    public interface IInventoryService : IInventoryPositionProvider
    {
        /// <summary>
        /// Creates a new product location position with the specified details.
        /// All positions created within the same service scope will use 
        /// the correct sequential position balances, even if not yet persisted to the database.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <param name="location">The location type where the position is created.</param>
        /// <param name="positionType">The type of position to create.</param>
        /// <param name="sideType">The side type of the position (buy/sell).</param>
        /// <param name="quantityUnits">The quantity units for the position.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The newly created product location position.</returns>
        Task<ProductLocationPosition> CreatePositionAsync(
            ProductId productId,
            TradeId relatedTradeId,
            LocationType location,
            PositionType positionType,
            PositionSideType sideType,
            QuantityUnits quantityUnits,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines if there is enough quantity available for a sell operation.
        /// </summary>
        /// <param name="location">The location type to check.</param>
        /// <param name="positionType">The type of position to check.</param>
        /// <param name="quantityPerProduct">Dictionary mapping product IDs to required quantities.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if there is enough quantity available; otherwise, false.</returns>
        Task<bool> HasEnoughQuantityForSellAsync(
            LocationType location,
            PositionType positionType,
            Dictionary<ProductId, QuantityUnits> quantityPerProduct,
            CancellationToken cancellationToken = default);
    }
}
