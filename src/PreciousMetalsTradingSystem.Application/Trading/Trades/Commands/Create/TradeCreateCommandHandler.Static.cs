using FluentValidation.Results;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create
{
    public partial class TradeCreateCommandHandler
    {
        /// <summary>
        /// Validates that all requested products exist and are configured for the specified location. 
        /// Throws a <see cref="ValidationException"/> if any product is missing or improperly configured.
        /// </summary>
        private static void ValidateProductsAndConfigurationsExistsOrThrow(
            LocationType location,
            IEnumerable<Product> products,
            IEnumerable<ProductId> requestedProducts)
        {
            var productDictionary = products.ToDictionary(p => p.Id, p => p);
            var validationFailures = new List<ValidationFailure>();

            // Validate not existing products
            requestedProducts
                .Where(rp => !productDictionary.ContainsKey(rp))
                .ToList()
                .ForEach(rp => validationFailures.Add(new ValidationFailure(
                    "Products",
                    $"Product with ID '{rp}' is not found.")));

            // Validate configurations for existing products
            requestedProducts
                .Where(rp => productDictionary.ContainsKey(rp))
                .ToList()
                .ForEach(rp =>
                {
                    var product = productDictionary[rp];
                    var configuration = product
                        .LocationConfigurations
                        .SingleOrDefault(lc => lc.LocationType == location);

                    if (configuration == null)
                    {
                        validationFailures.Add(new ValidationFailure(
                            "LocationConfigurations",
                            $"Product with ID '{rp}' is not configured for location '{location}'."));
                    }
                });

            validationFailures
                .Throw(() => new ValidationException(validationFailures))
                .IfNotEmpty();
        }

        /// <summary>
        /// Calculates the total requested weight in ounces for each <see cref="MetalType"/> 
        /// based on the provided products and trade items.
        /// </summary>
        /// <returns>
        /// A dictionary where the key is the <see cref="MetalType"/> and the value is the total weight in ounces.
        /// </returns>
        /// <remarks>
        /// Throws an exception if a trade item references a product not found in the provided list.
        /// </remarks>
        private static Dictionary<MetalType, QuantityOunces> CalculateQuantityOuncesPerMetalType(
            IEnumerable<Product> productsForTrading,
            IEnumerable<Models.TradeItemRequestBase> requestTradeItems)
        {
            var productsDictionary = productsForTrading.ToDictionary(p => p.Id, p => p);

            // Ensure all trade items have a corresponding product
            requestTradeItems.ToList().ForEach(tradeItem =>
            {
                var productId = new ProductId(tradeItem.ProductId);
                productsDictionary.ContainsKey(productId)
                    .Throw(() =>
                        new InvalidOperationException($"Product with ID '{tradeItem.ProductId}' " +
                        $"does not exist in the provided product list."))
                    .IfFalse();
            });

            // Map trade items to their corresponding ProductId once
            var tradeItemWithProductIds = requestTradeItems
                .Select(tradeItem => new
                {
                    ProductId = new ProductId(tradeItem.ProductId),
                    TradeItem = tradeItem
                })
                .Where(item => productsDictionary.ContainsKey(item.ProductId));

            // Calculate requested quantities
            var requestedQuantities = tradeItemWithProductIds
                .GroupBy(
                    item => productsDictionary[item.ProductId].MetalType,
                    item => item.TradeItem.UnitQuantity * productsDictionary[item.ProductId].WeightInOz.Value)
                .ToDictionary(
                    group => group.Key,
                    group => new QuantityOunces(group.Sum(quantity => quantity)));

            return requestedQuantities;
        }

        private static Dictionary<Product, QuantityUnits> CalculateQuantityUnitsPerProduct(
            IEnumerable<Product> productsForTrading,
            IEnumerable<Models.TradeItemRequestBase> tradeItems)
        {
            // Convert products to a dictionary for efficient lookups
            var productsDictionary = productsForTrading.ToDictionary(p => p.Id, p => p);

            // Ensure all trade items have a corresponding product
            tradeItems.ToList().ForEach(tradeItem =>
            {
                var productId = new ProductId(tradeItem.ProductId);
                productsDictionary.ContainsKey(productId)
                    .Throw(() =>
                        new InvalidOperationException($"Product with ID '{tradeItem.ProductId}' " +
                        $"does not exist in the provided product list."))
                    .IfFalse();
            });

            // Map dealer trade items to products and their quantities
            var quantityPerProduct = tradeItems
                .ToDictionary(
                    tradeItem => productsDictionary[new ProductId(tradeItem.ProductId)],
                    tradeItem => new QuantityUnits(tradeItem.UnitQuantity)
                );

            return quantityPerProduct;
        }
    }
}
