using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using LinqKit;
using MediatR;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create
{
    public partial class TradeCreateCommandHandler : IRequestHandler<DealerTradeCreateCommand, Guid>
    {
        public async Task<Guid> Handle(DealerTradeCreateCommand request, CancellationToken cancellationToken)
        {
            //TODO: Edge Case handler if persit to DB fails - we have to unhedge what we previously hedged

            var requestedProductsIds = request.Items.Select(x => new ProductId(x.ProductId));

            var products = await GetProductsForTradingAsync(requestedProductsIds, cancellationToken);

            ValidateProductsAndConfigurationsExistsOrThrow(request.Location, products, requestedProductsIds);

            if (request.SideType == SideType.Buy)
            {
                await ValidateAgainstCashBalanceOrThrowAsync(
                    request.Items.Select(item =>
                        (
                            new ProductId(item.ProductId),
                            new QuantityUnits(item.UnitQuantity),
                            new Money(item.DealerPricePerOz))
                        ),
                    products,
                    cancellationToken);
            }
            else
            {
                var quantityPerProduct = CalculateQuantityUnitsPerProduct(products, request.Items);
                await ValidateAgainstPositionsBalanceOrThrowAsync(request.Location, quantityPerProduct, cancellationToken);
            }

            SpotDeferredTrade? spotDeferredTrade = null;
            var trade = await CreateEmptyDealerTradeAsync(request.TradeDate, request.SideType, request.Location, request.Note, cancellationToken);

            if (request.AutoHedge)
            {
                var quantityPerMetalType = CalculateQuantityOuncesPerMetalType(products, request.Items);
                spotDeferredTrade = await HedgeAndSubmitSpotDeferredTradeAsync(request.Location, request.SideType, trade.TradeNumber, quantityPerMetalType, cancellationToken);
            }

            await CompleteAndSubmitDealerTradeAsync(request, trade, products, spotDeferredTrade, cancellationToken);

            await CreateAndSubmitFinancialTransactionAsync(trade, cancellationToken);

            await CreateAndSubmitPositionsAsync(trade, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return trade.Id.Value;
        }

        private async Task<SpotDeferredTrade> CreateAndSubmitSpotDeferredTradeAsync(
            LocationType location,
            SideType side,
            string tradeConfirmationReference,
            Dictionary<MetalType, (Money spotPricePerOz, QuantityOunces quantity)> quantityAndSpotPricesPerOzPerMetalType,
            CancellationToken cancellationToken)
        {
            var hedgingAccount = await GetHedgingAccountOrThrowAsync(location, cancellationToken);

            var spotDeferredTrade = SpotDeferredTrade.Create(
                hedgingAccount.Id,
                tradeConfirmationReference,
                side,
                DateTime.UtcNow.ConvertUtcToEstDateOnly());

            var spotDeferredTradeItems = quantityAndSpotPricesPerOzPerMetalType
                .Select(item => SpotDeferredTradeItem.Create(
                    item.Key,
                    item.Value.spotPricePerOz,
                    item.Value.quantity)).ToList();

            spotDeferredTradeItems.ForEach(spotDeferredTrade.AddItem);

            await _spotDeferredTradeRepository.AddAsync(spotDeferredTrade, cancellationToken);

            return spotDeferredTrade;

        }

        private async Task<SpotDeferredTrade> HedgeAndSubmitSpotDeferredTradeAsync(
            LocationType location,
            SideType side,
            string tradeNumber,
            Dictionary<MetalType, QuantityOunces> quantityPerMetalType,
            CancellationToken cancellationToken)
        {
            var hedgeResult = await _hedgingService.HedgeAsync(
                quantityPerMetalType,
                side,
                location,
                tradeNumber,
                cancellationToken);

            Dictionary<MetalType, (Money spotPricePerOz, QuantityOunces quantityOunces)> dic = [];
            quantityPerMetalType.ForEach(item =>
            {
                var metal = item.Key;
                var quantity = item.Value;
                var spotPricePerOz = hedgeResult.SpotPricesPerOz.GetValueOrDefault(metal);
                dic.Add(metal, (new Money(spotPricePerOz), quantity));
            });

            var spotDeferredTrade = await CreateAndSubmitSpotDeferredTradeAsync(
                location,
                side.ToOppositeSide(),
                hedgeResult.TradeConfirmationNumber,
                dic,
                cancellationToken);

            return spotDeferredTrade;
        }

        private async Task<Trade> CreateEmptyDealerTradeAsync(
            DateTime tradeDate,
            SideType sideType,
            LocationType locationType,
            string? note,
            CancellationToken cancellationToken = default)
        {
            var tradeDateTime = tradeDate.Date.AddHours(12);

            var settlementDate = await _tradingService.CalculateFinancialSettlementDateAsync(
                    tradeDateTime,
                    CalendarType.FederalReserve,
                    cancellationToken);

            var trade = Trade.Create(
                TradeType.DealerTrade,
                sideType,
                locationType,
                DateOnly.FromDateTime(tradeDate),
                settlementDate,
                note);

            trade.MarkAsConfirmed();

            return trade;
        }

        private async Task CompleteAndSubmitDealerTradeAsync(
            DealerTradeCreateCommand request,
            Trade trade,
            IEnumerable<Product> products,
            SpotDeferredTrade? spotDeferredTrade = null,
            CancellationToken cancellationToken = default)
        {
            var spotPricesPerOzByMetalType = new Dictionary<MetalType, Money>();
            if (spotDeferredTrade is not null)
            {
                trade.SetSpotDeferredTrade(spotDeferredTrade.Id);
                spotPricesPerOzByMetalType = spotDeferredTrade.GetPricesPerOzByMetalType();
            }

            var productsDictionary = products.ToDictionary(p => p.Id);
            request.Items.ToList().ForEach(item =>
            {
                var productId = new ProductId(item.ProductId);
                var product = productsDictionary
                    .GetValueOrDefault(productId)
                    .ThrowIfNull(() => new InvalidOperationException($"Product with ID '{item.ProductId}' not found."));

                Money spotPricePerOz = request.AutoHedge ? spotPricesPerOzByMetalType[product.Value.MetalType] : new Money(item.SpotPricePerOz!.Value);

                var tradeItem = TradeItem.Create(
                    tradeSide: request.SideType,
                    productId: product.Value.Id,
                    productWeightInOz: product.Value.WeightInOz,
                    quantityUnits: new QuantityUnits(item.UnitQuantity),
                    spotPricePerOz: spotPricePerOz,
                    tradePricePerOz: new Money(item.DealerPricePerOz),
                    premiumPerOz: product.Value.GetPremium(request.Location, request.SideType)!,
                    effectivePricePerOz: product.Value.CalculatePricePerOz(
                        new Money(item.DealerPricePerOz),
                        request.Location,
                        request.SideType));

                trade.AddItem(tradeItem);
            });

            await _tradeRepository.AddAsync(trade, cancellationToken);
        }
    }
}
