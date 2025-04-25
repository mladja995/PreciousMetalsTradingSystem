using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.DomainServices;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Inventory.Services
{
    /// <summary>
    /// Provides inventory services for managing product positions and quantities.
    /// This service maintains an in-memory cache of the latest positions for performance
    /// and consistency when creating multiple positions within a single request scope.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IRepository<ProductLocationPosition, ProductLocationPositionId> _repository;
        private readonly IInventoryPositionValidator _positionValidator;

        /// <summary>
        /// In-memory cache of the most recent position balance per product, location, and position type.
        /// This ensures consistency when creating multiple positions in sequence
        /// before they are persisted to the database.
        /// </summary>
        private readonly ConcurrentDictionary<(ProductId, LocationType, PositionType), PositionQuantityUnits> _positionBalanceCache = new();

        /// <summary>
        /// In-memory tracking of created positions that haven't been persisted yet.
        /// </summary>
        private readonly ConcurrentDictionary<(LocationType, PositionType), List<ProductLocationPosition>> _pendingPositions = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryService"/> class.
        /// </summary>
        /// <param name="repository">The repository for product location positions.</param>
        /// <param name="positionValidator">The validator for inventory positions.</param>
        public InventoryService(
            IRepository<ProductLocationPosition, ProductLocationPositionId> repository,
            IInventoryPositionValidator positionValidator)
        {
            _repository = repository.ThrowIfNull().Value;
            _positionValidator = positionValidator.ThrowIfNull().Value;
        }

        /// <inheritdoc/>
        public async Task<PositionQuantityUnits> GetRunningPositionBalanceAsync(
            ProductId productId,
            LocationType location,
            PositionType positionType,
            CancellationToken cancellationToken = default)
        {
            // If we already have this position balance in the cache, return it
            if (_positionBalanceCache.TryGetValue((productId, location, positionType), out var cachedBalance))
            {
                return cachedBalance;
            }

            // Otherwise, fetch from the database
            var latestPosition = await _repository.StartQuery(readOnly: true)
                .Where(p => p.ProductId == productId && p.LocationType == location && p.Type == positionType)
                .OrderByDescending(p => p.TimestampUtc)
                .Select(p => p.PositionUnits)
                .FirstOrDefaultAsync(cancellationToken);

            var currentBalance = latestPosition is null
                ? new PositionQuantityUnits(0)
                : latestPosition;

            // Cache the result
            _positionBalanceCache[(productId, location, positionType)] = currentBalance;

            return currentBalance;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductLocationPosition>> GetRunningPositionsAsync(
            LocationType location,
            DateTime? onDateUtc = null,
            CancellationToken cancellationToken = default)
        {
            return await _repository.StartQuery(readOnly: true)
                .Where(p => p.LocationType == location && (onDateUtc == null || onDateUtc <= p.TimestampUtc))
                .Include(p => p.Product)
                .GroupBy(p => new { p.Type, p.ProductId })
                .Select(g => g.OrderByDescending(p => p.TimestampUtc).First())
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ProductLocationPosition> CreatePositionAsync(
            ProductId productId,
            TradeId relatedTradeId,
            LocationType location,
            PositionType positionType,
            PositionSideType sideType,
            QuantityUnits quantityUnits,
            CancellationToken cancellationToken = default)
        {
            productId.ThrowIfNull();
            quantityUnits.ThrowIfNull();

            // Get the current position balance
            var currentPositionBalance = await GetRunningPositionBalanceAsync(
                productId,
                location,
                positionType,
                cancellationToken);

            // Create the position directly using the entity's Create method
            var newPosition = ProductLocationPosition.Create(
                productId,
                relatedTradeId,
                location,
                sideType,
                positionType,
                quantityUnits,
                currentPositionBalance);

            // Update the position balance cache with the new balance
            _positionBalanceCache[(productId, location, positionType)] = newPosition.PositionUnits;

            // Track this position as pending for this location and position type
            var key = (location, positionType);
            if (!_pendingPositions.TryGetValue(key, out var positions))
            {
                positions = [];
                _pendingPositions[key] = positions;
            }
            positions.Add(newPosition);

            return newPosition;
        }

        /// <inheritdoc/>
        public async Task<bool> HasEnoughQuantityForSellAsync(
            LocationType location,
            PositionType positionType,
            Dictionary<ProductId, QuantityUnits> quantityPerProduct,
            CancellationToken cancellationToken = default)
        {
            quantityPerProduct.ThrowIfNull();

            // Validate that each product has enough quantity
            foreach (var requested in quantityPerProduct)
            {
                var productId = requested.Key;
                var requestedQuantity = requested.Value;

                // Get the current position balance for this product
                var currentPosition = await GetRunningPositionBalanceAsync(
                    productId,
                    location,
                    positionType,
                    cancellationToken);

                // Use the validator to check if the position is sufficient for the requested quantity
                if (!_positionValidator.IsSufficientForSell(requestedQuantity, currentPosition))
                {
                    return false;
                }
            }

            // If all checks pass, there is enough quantity for all products
            return true;
        }
    }
}
