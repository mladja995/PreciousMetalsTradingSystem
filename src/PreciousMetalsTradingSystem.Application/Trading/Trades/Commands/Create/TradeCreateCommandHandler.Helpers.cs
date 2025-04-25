using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Financials.Exceptions;
using PreciousMetalsTradingSystem.Application.Inventory.Exceptions;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create
{
    public partial class TradeCreateCommandHandler
    {
        /// <summary>
        /// Asynchronously retrieves products for trading, including their location configurations.
        /// Products with the specified IDs that do not exist will not be included in the result.
        /// </summary>
        /// <returns>A collection of products matching the specified IDs.</returns>
        private async Task<IEnumerable<Product>> GetProductsForTradingAsync(
            IEnumerable<ProductId> productIds,
            CancellationToken cancellationToken)
        {
            return await _productRepository
                .StartQuery()
                .Include(p => p.LocationConfigurations)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
        }

        private async Task<HedgingAccount> GetHedgingAccountOrThrowAsync(
            LocationType location,
            CancellationToken cancellationToken)
        {
            // TODO: We should cache location hedging account configurations and pull it from there

            var locationHedgingAccountConfiguration = await
                _locationHedgingAccountRepository.GetByIdOrThrowAsync(
                    id: new LocationHedgingAccountConfigurationId(location),
                    cancellationToken: cancellationToken,
                    includes: x => x.HedgingAccount);

            return locationHedgingAccountConfiguration.HedgingAccount;
        }

        /// <summary>
        /// Validates if the cash balance is sufficient for the given trade items based on their price per ounce and quantity.
        /// Throws an exception if the balance is insufficient.
        /// </summary>
        /// <param name="tradeItems">A collection of trade items with ProductId, UnitQuantity, and PricePerOz.</param>
        /// <param name="productsForTrading">A collection of products available for trading.</param>
        /// <param name="cancellationToken">A cancellation token for async operation.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task ValidateAgainstCashBalanceOrThrowAsync(
            IEnumerable<(ProductId ProductId, QuantityUnits UnitQuantity, Money PricePerOz)> tradeItems,
            IEnumerable<Product> productsForTrading,
            CancellationToken cancellationToken)
        {
            var balanceType = BalanceType.Effective;

            var totalAmount = tradeItems
                .Join(productsForTrading,
                    item => item.ProductId,
                    product => product.Id,
                    (item, product) => new { Item = item, Product = product })
                .Sum(x => x.Item.UnitQuantity * x.Product.WeightInOz * x.Item.PricePerOz);

            var hashEnoughCash = await _financialsService.HasEnoughCashForBuyAsync(
                balanceType,
                new Money(totalAmount),
                cancellationToken);

            hashEnoughCash
                .Throw(() => new NotEnoughCashForBuyException(balanceType))
                .IfFalse();
        }

        /// <summary>
        /// Validates if the inventory has sufficient quantity of products available for trading at the specified location.
        /// Throws an exception if the quantity is insufficient.
        /// </summary>
        /// <param name="location">The location where the inventory is being checked.</param>
        /// <param name="quantityPerProduct">A dictionary mapping products to their required quantities.</param>
        /// <param name="cancellationToken">A cancellation token for async operation.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task ValidateAgainstPositionsBalanceOrThrowAsync(
            LocationType location,
            Dictionary<Product, QuantityUnits> quantityPerProduct,
            CancellationToken cancellationToken)
        {
            var positionType = PositionType.AvailableForTrading;

            var hasEnoughQuantity = await _inventoryService
                .HasEnoughQuantityForSellAsync(
                    location,
                    PositionType.AvailableForTrading,
                    quantityPerProduct.ToDictionary(x => x.Key.Id, x => x.Value),
                    cancellationToken);

            hasEnoughQuantity
                .Throw(() => new NotEnoughQuantityForSellException(positionType))
                .IfFalse();
        }

        /// <summary>
        /// Creates a financial transaction for a given trade and submits it to the repository.
        /// </summary>
        /// <param name="trade">The trade for which the financial transaction is being created.</param>
        /// <param name="cancellationToken">A cancellation token for async operation.</param>
        /// <returns>A Task that represents the asynchronous operation and returns the created financial transaction.</returns>
        private async Task<FinancialTransaction> CreateAndSubmitFinancialTransactionAsync(
            Trade trade,
            CancellationToken cancellationToken)
        {
            var transaction = await _financialsService.CreateFinancialTransactionAsync(
                trade.Type.ToActivityType(),
                trade.Side.ToTransactionSideType(),
                BalanceType.Effective,
                trade.GetTotalAmount(),
                trade.Id,
                cancellationToken);

            await _financialTransactionRepository.AddAsync(transaction, cancellationToken);

            return transaction;
        }

        /// <summary>
        /// Creates product location positions for a given trade and submits them to the repository.
        /// </summary>
        /// <param name="trade">The trade for which the positions are being created.</param>
        /// <param name="cancellationToken">A cancellation token for async operation.</param>
        /// <returns>A Task that represents the asynchronous operation and returns a collection of created product location positions.</returns>
        private async Task<IEnumerable<ProductLocationPosition>> CreateAndSubmitPositionsAsync(
            Trade trade,
            CancellationToken cancellationToken)
        {
            List<ProductLocationPosition> positions = [];

            foreach (var item in trade.Items)
            {
                var position = await _inventoryService.CreatePositionAsync(
                    item.ProductId,
                    trade.Id,
                    trade.LocationType,
                    PositionType.AvailableForTrading,
                    trade.Side.ToPositionSideType(),
                    item.QuantityUnits,
                    cancellationToken);

                await _productLocationPositionRespository.AddAsync(position, cancellationToken);
                positions.Add(position);
            }

            return positions;
        }
    }
}
