using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Models;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MediatR;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create
{
    public partial class TradeCreateCommandHandler : IRequestHandler<ClientTradeCreateCommand, ClientTradeResult>
    {
        public async Task<ClientTradeResult> Handle(ClientTradeCreateCommand request, CancellationToken cancellationToken)
        {
            SideType side = request.SideType.ToSideType();
            var requestedProductsIds = request.Items.Select(x => new ProductId(x.ProductId));

            var products = await GetProductsForTradingAsync(requestedProductsIds, cancellationToken);

            ValidateProductsAndConfigurationsExistsOrThrow(request.Location, products, requestedProductsIds);

            if (side == SideType.Buy)
            {
                await ValidateAgainstCashBalanceOrThrowAsync(
                    request.Items.Select(item =>
                        (
                            new ProductId(item.ProductId),
                            new QuantityUnits(item.UnitQuantity),
                            products
                                .First(x => x.Id == item.ProductId)
                                .CalculatePricePerOz(
                                    new Money(item.SpotPricePerOz),
                                    request.Location,
                                    side))
                        ),
                    products,
                    cancellationToken);
            }
            else
            {
                var quantityPerProduct = CalculateQuantityUnitsPerProduct(products, request.Items);
                await ValidateAgainstPositionsBalanceOrThrowAsync(request.Location, quantityPerProduct, cancellationToken);
            }

            var trade = await CreateAndSubmitClientTradeAsync(request, products, cancellationToken);

            await CreateAndSubmitFinancialTransactionAsync(trade, cancellationToken);

            await CreateAndSubmitPositionsAsync(trade, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ClientTradeResult { Id = trade.Id, TradeNumber = trade.TradeNumber };
        }

        private async Task<Trade> CreateAndSubmitClientTradeAsync(
            ClientTradeCreateCommand request,
            IEnumerable<Product> products,
            CancellationToken cancellationToken = default)
        {
            var side = request.SideType.ToSideType();
            var tradeDate = DateOnly.FromDateTime(request.TradeDate);
            var tradeDateTime = request.TradeDate.Date.AddHours(12);

            var settlementDate = await _tradingService.CalculateFinancialSettlementDateAsync(
                tradeDateTime,
                CalendarType.FederalReserve,
                cancellationToken);

            var trade = Trade.Create(
                tradeType: TradeType.ClientTrade,
                sideType: side,
                locationType: request.Location,
                tradeDate: tradeDate,
                financialsSettleOn: settlementDate,
                request.Note);

            var productsDictionary = products.ToDictionary(p => p.Id);
            request.Items.ToList().ForEach(item =>
            {
                var productId = new ProductId(item.ProductId);
                var product = productsDictionary
                    .GetValueOrDefault(productId)
                    .ThrowIfNull(() => new InvalidOperationException($"Product with ID '{item.ProductId}' not found."));

                var spotPricePerOz = new Money(item.SpotPricePerOz);

                var tradeItem = TradeItem.Create(
                    tradeSide: side,
                    productId: product.Value.Id,
                    productWeightInOz: product.Value.WeightInOz,
                    quantityUnits: new QuantityUnits(item.UnitQuantity),
                    spotPricePerOz: spotPricePerOz,
                    tradePricePerOz: spotPricePerOz,
                    premiumPerOz: product.Value.GetPremium(request.Location, side)!,
                    effectivePricePerOz: product.Value.CalculatePricePerOz(
                        spotPricePerOz,
                        request.Location,
                        side));

                trade.AddItem(tradeItem);
            });

            await _tradeRepository.AddAsync(trade, cancellationToken);

            return trade;
        }
    }
}
