using FluentValidation;
using FluentValidation.Results;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Common.Options;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Exceptions;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Application.Heding.SpotDeferredTrades.Services;
using PreciousMetalsTradingSystem.Application.Inventory.Exceptions;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Models;
using PreciousMetalsTradingSystem.Application.Trading.Pricing.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.RequestQuote
{
    public class QuoteRequestCommandHandler : IRequestHandler<QuoteRequestCommand, Quote>
    {
        private readonly IRepository<TradeQuote, TradeQuoteId> _tradeQuoteRepository;
        private readonly IRepository<Product, ProductId> _productRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IFinancialsService _financialsService;
        private readonly IHedgingService _hedgingService;
        private readonly IPricingService _pricingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApiSettingsOptions _apiSettingsOptions;

        public QuoteRequestCommandHandler(
            IRepository<TradeQuote, TradeQuoteId> tradeQuoteRepository,
            IRepository<Product, ProductId> productRepository,
            IInventoryService inventoryService,
            IFinancialsService financialsService,
            IHedgingService hedgingService,
            IPricingService pricingService,
            IUnitOfWork unitOfWork,
            IOptions<ApiSettingsOptions> options)
        {
            _tradeQuoteRepository = tradeQuoteRepository;
            _productRepository = productRepository;
            _inventoryService = inventoryService;
            _financialsService = financialsService;
            _hedgingService = hedgingService;
            _pricingService = pricingService;
            _unitOfWork = unitOfWork;
            _apiSettingsOptions = options.Value;
        }

        public async Task<Quote> Handle(QuoteRequestCommand request, CancellationToken cancellationToken)
        {
            var requestedProductsSKUs = request.Items.Select(x => new SKU(x.ProductSKU));

            var products = await GetProductsForTradingAsync(requestedProductsSKUs, cancellationToken);

            ValidateProductsAndConfigurationsExistsOrThrow(
                request.Location, 
                request.SideType.ToSideType(),
                products, 
                requestedProductsSKUs);

            var quantityPerProduct = CalculateQuantityUnitsPerProduct(
                products,
                request.Items);

            await ValidateCanWeTradeRequestedQuantityOrThrow(
                request.Location,
                request.SideType.ToSideType(),
                quantityPerProduct,
                cancellationToken);

            var tradeQuote = await CreateAndSubmitTradeQuoteAsync(
                request,
                products,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return tradeQuote.ToQuote();
        }

        private async Task<IEnumerable<Product>> GetProductsForTradingAsync(
            IEnumerable<SKU> productsSKUs,
            CancellationToken cancellationToken)
        {
            return await _productRepository
                .StartQuery()
                .Include(p => p.LocationConfigurations)
                .Where(p => productsSKUs.Contains(p.SKU))
                .ToListAsync(cancellationToken);
        }

        private static void ValidateProductsAndConfigurationsExistsOrThrow(
            LocationType location,
            SideType side,
            IEnumerable<Product> products,
            IEnumerable<SKU> requestedProducts)
        {
            var productDictionary = products.ToDictionary(p => p.SKU, p => p);
            var validationFailures = new List<ValidationFailure>();

            // Validate not existing products
            requestedProducts
                .Where(rps => !productDictionary.ContainsKey(rps))
                .ToList()
                .ForEach(rps => validationFailures.Add(new ValidationFailure(
                    "Products",
                    $"Product with SKU '{rps}' is not found.")));

            // Validate configurations for existing products
            requestedProducts
                .Where(productDictionary.ContainsKey)
                .ToList()
                .ForEach(rps =>
                {
                    var product = productDictionary[rps];

                    if (!product.IsAvailableForTrading(location, side))
                    {
                        validationFailures.Add(new ValidationFailure(
                            "LocationConfigurations",
                            $"Product with SKU '{rps}' is not configured for location '{location}'."));
                    }
                });

            validationFailures
                .Throw(() => new Common.Exceptions.ValidationException(validationFailures))
                .IfNotEmpty();
        }

        private static Dictionary<Product, QuantityUnits> CalculateQuantityUnitsPerProduct(
            IEnumerable<Product> productsForTrading,
            IEnumerable<QuoteRequestItem> quoteItems)
        {
            // Convert products to a dictionary for efficient lookups
            var productsDictionary = productsForTrading.ToDictionary(p => p.SKU, p => p);

            // Ensure all quote items have a corresponding product
            quoteItems.ToList().ForEach(quoteItem =>
            {
                var productSKU = new SKU(quoteItem.ProductSKU);
                productsDictionary.ContainsKey(productSKU)
                    .Throw(() =>
                        new InvalidOperationException($"Product with SKU '{quoteItem.ProductSKU}' " +
                        $"does not exist in the provided product list."))
                    .IfFalse();
            });

            // Map quote items to products and their quantities
            var quantityPerProduct = quoteItems
                .ToDictionary(
                    quoteItem => productsDictionary[new SKU(quoteItem.ProductSKU)],
                    quoteItem => new QuantityUnits(quoteItem.QuantityUnits)
                );

            return quantityPerProduct;
        }

        private static Dictionary<MetalType, QuantityOunces> CalculateQuantityOuncesPerMetalType(
            IEnumerable<Product> productsForTrading,
            IEnumerable<QuoteRequestItem> quoteItems)
        {
            var productsDictionary = productsForTrading.ToDictionary(p => p.SKU, p => p);

            // Ensure all trade items have a corresponding product
            quoteItems.ToList().ForEach(quoteItem =>
            {
                var productSKU = new SKU(quoteItem.ProductSKU);
                productsDictionary.ContainsKey(productSKU)
                    .Throw(() =>
                        new InvalidOperationException($"Product with SKU '{quoteItem.ProductSKU}' " +
                        $"does not exist in the provided product list."))
                    .IfFalse();
            });

            // Map quote items to their corresponding ProductSKU once
            var quoteItemWithProductSKUs = quoteItems
                .Select(quoteItem => new
                {
                    ProductSKU = new SKU(quoteItem.ProductSKU),
                    QuoteItem = quoteItem
                })
                .Where(item => productsDictionary.ContainsKey(item.ProductSKU));

            // Calculate requested quantities
            var requestedQuantities = quoteItemWithProductSKUs
                .GroupBy(
                    item => productsDictionary[item.ProductSKU].MetalType,
                    item => item.QuoteItem.QuantityUnits * productsDictionary[item.ProductSKU].WeightInOz.Value)
                .ToDictionary(
                    group => group.Key,
                    group => new QuantityOunces(group.Sum(quantity => quantity)));

            return requestedQuantities;
        }

        private async Task ValidateCanWeTradeRequestedQuantityOrThrow(
            LocationType location,
            SideType side,
            Dictionary<Product, QuantityUnits> quantityPerProduct,
            CancellationToken cancellationToken)
        {
            if (side == SideType.Buy)
            {
                await ValidateAgainstCashBalanceOrThrowAsync(
                    location,
                    quantityPerProduct,
                    cancellationToken);
            }
            else
            {
                await ValidateAgainstPositionsBalanceOrThrowAsync(
                    location,
                    quantityPerProduct,
                    cancellationToken);
            }
        }

        private async Task ValidateAgainstCashBalanceOrThrowAsync(
            LocationType location,
            Dictionary<Product, QuantityUnits> quantityPerProduct,
            CancellationToken cancellationToken)
        {
            var balanceType = BalanceType.Effective;
            var preliminarSpotPrices = await _pricingService.GetSpotPricesAsync(
                location,
                SideType.Buy,
                cancellationToken);

            var totalAmount = quantityPerProduct.Select(qpp =>
            {
                return
                    qpp.Value *
                    qpp.Key.WeightInOz *
                    qpp.Key.CalculatePricePerOz(
                        spotPricePerOz: new Money(
                            preliminarSpotPrices.Single(
                                psp => psp.ProductSKU.Equals(qpp.Key.SKU)).SpotPricePerOz),
                        location: location,
                        side: SideType.Buy);
            }).Sum();

            var hashEnoughCash = await _financialsService.HasEnoughCashForBuyAsync(
                balanceType,
                new Money(totalAmount),
                cancellationToken);

            hashEnoughCash
                .Throw(() => new NotEnoughCashForBuyException(balanceType))
                .IfFalse();
        }

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

        private async Task<TradeQuote> CreateAndSubmitTradeQuoteAsync(
            QuoteRequestCommand request,
            IEnumerable<Product> products,
            CancellationToken cancellationToken)
        {
            var productsDictionary = products.ToDictionary(p => p.SKU, p => p);

            var hedgeQuote = await _hedgingService.GetHedgeQuoteAsync(
                CalculateQuantityOuncesPerMetalType(products, request.Items),
                request.SideType.ToSideType(),
                request.Location,
                cancellationToken);

            var quoteIssuedAtUtc = DateTime.UtcNow;
            var quoteValidUntilUtc = quoteIssuedAtUtc.AddSeconds(_apiSettingsOptions.QuoteValidityPeriodInSeconds);

            var tradeQuote = TradeQuote.Create(
                hedgeQuote.QuoteKey,
                quoteIssuedAtUtc,
                quoteValidUntilUtc,
                request.SideType.ToSideType(),
                request.Location,
                request.Note);

            request.Items.ForEach(item =>
            {
                var product = productsDictionary[new SKU(item.ProductSKU)];
                var spotPricePerOz = new Money(hedgeQuote.SpotPricesPerOz[product.MetalType]);
                var premiumPerOz = product.GetPremium(request.Location, request.SideType.ToSideType())!;
                var effectivePricePerOz = product.CalculatePricePerOz(spotPricePerOz, request.Location, request.SideType.ToSideType());

                var quoteItem = TradeQuoteItem.Create(
                    product: product,
                    quantityUnits: new QuantityUnits(item.QuantityUnits),
                    spotPricePerOz: spotPricePerOz,
                    premiumPricePerOz: premiumPerOz,
                    effectivePricePerOz: effectivePricePerOz);
                
                tradeQuote.AddItem(quoteItem);
            });

            await _tradeQuoteRepository.AddAsync(tradeQuote, cancellationToken);

            return tradeQuote;
        }
    }
}
