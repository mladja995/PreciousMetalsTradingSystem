using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Application.Inventory.Services
{
    /// <summary>
    /// Defines a provider that supplies the current inventory position information for products.
    /// This interface is used to obtain running position data for inventory operations.
    /// </summary>
    public interface IInventoryPositionProvider
    {
        /// <summary>
        /// Gets the running position balance quantity for a specific product at a specified location.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <param name="location">The location type where the product is positioned.</param>
        /// <param name="positionType">The type of position to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The current position quantity units for the product.</returns>
        Task<PositionQuantityUnits> GetRunningPositionBalanceAsync(
            ProductId productId,
            LocationType location,
            PositionType positionType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the running positions for all products at a specified location.
        /// </summary>
        /// <param name="location">The location type to query positions for.</param>
        /// <param name="onDateUtc">Optional date filter to retrieve positions as of a specific date.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A collection of product location positions.</returns>
        Task<IEnumerable<ProductLocationPosition>> GetRunningPositionsAsync(
            LocationType location,
            DateTime? onDateUtc = null,
            CancellationToken cancellationToken = default);
    }
}
