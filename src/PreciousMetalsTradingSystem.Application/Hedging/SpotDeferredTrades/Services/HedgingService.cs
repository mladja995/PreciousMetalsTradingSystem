using PreciousMetalsTradingSystem.Application.AMark.Models;
using PreciousMetalsTradingSystem.Application.AMark.Services;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Application.Heding.SpotDeferredTrades.Services
{
    public class HedgingService : IHedgingService
    {
        private readonly IAMarkTradingServiceFactory _amarkTradingServiceFactory;

        public HedgingService(IAMarkTradingServiceFactory amarkTradingServiceFactory)
        {
            _amarkTradingServiceFactory = amarkTradingServiceFactory;
        }

        public async Task<HedgeResult> HedgeAsync(
            Dictionary<MetalType, QuantityOunces> quantityItems, 
            SideType side, 
            LocationType locationType,
            string referenceNumber,
            CancellationToken cancellationToken)
        {
            var amarkTradingService = await _amarkTradingServiceFactory.CreateAsync(locationType, cancellationToken);
            
            var onlineQuoteRequest = CreateQuoteRequest(quantityItems, side);
            var quoteResponse = await amarkTradingService.RequestOnlineQuoteAsync(onlineQuoteRequest, cancellationToken);

            var tradeRequest = CreateTradeRequest(quoteResponse.QuoteKey, referenceNumber);
            var tradeResponse = await amarkTradingService.RequestOnlineTradeAsync(tradeRequest, cancellationToken);

            return new HedgeResult
            {
                QuoteKey = quoteResponse.QuoteKey,
                TradeConfirmationNumber = tradeResponse.TicketNumber,
                SpotPricesPerOz = quoteResponse.QuoteProductsPricingList
                    .Select(x => new 
                    { 
                        MetalType = x.ProductCode.ToMetalTypeFromAMarkSpotDeferredProductCode(), 
                        x.SpotPrice 
                    })
                    .ToDictionary(x => x.MetalType, y => y.SpotPrice)
            };
        }


        public async Task<GetHedgeQuoteResult> GetHedgeQuoteAsync(
            Dictionary<MetalType, QuantityOunces> quantityItems, 
            SideType side, 
            LocationType locationType, 
            CancellationToken cancellationToken = default)
        {
            var amarkTradingService = await _amarkTradingServiceFactory.CreateAsync(locationType, cancellationToken);
            
            var onlineQuoteRequest = CreateQuoteRequest(quantityItems, side);
            var quoteResponse = await amarkTradingService.RequestOnlineQuoteAsync(onlineQuoteRequest, cancellationToken);

            return new GetHedgeQuoteResult
            {
                QuoteKey = quoteResponse.QuoteKey,
                SpotPricesPerOz = quoteResponse.QuoteProductsPricingList
                    .Select(x => new
                    {
                        MetalType = x.ProductCode.ToMetalTypeFromAMarkSpotDeferredProductCode(),
                        x.SpotPrice
                    })
                    .ToDictionary(x => x.MetalType, y => y.SpotPrice)
            };
        }

        public async Task<ExecuteHedgeQuoteResult> ExecuteHedgeQuoteAsync(
            LocationType locationType,
            string quoteKey,
            string referenceNumber,
            CancellationToken cancellationToken)
        {
            var amarkTradingService = await _amarkTradingServiceFactory.CreateAsync(locationType, cancellationToken);

            var tradeRequest = CreateTradeRequest(quoteKey, referenceNumber);
            var tradeResponse = await amarkTradingService.RequestOnlineTradeAsync(tradeRequest, cancellationToken);

            return new ExecuteHedgeQuoteResult
            {
                QuoteKey = quoteKey,
                TradeConfirmationNumber = tradeResponse.TicketNumber
            };
        }

        private static OnlineQuoteRequest CreateQuoteRequest(
            Dictionary<MetalType, QuantityOunces> quantityItems, 
            SideType side)
        {
            return new OnlineQuoteRequest
            {
                OrderType = side.ToAMarkOrderType(),
                ProductQuoteItems = quantityItems
                    .Select(x => new ProductQuoteItem
                    {
                        ProductCode = x.Key.ToAMarkSpotDeferredProductCode(),
                        ProductQuantity = x.Value,
                    }).ToList(),
            };
        }

        private static OnlineTradeRequest CreateTradeRequest(string quoteKey, string tpConfirmNo)
        {
            return new OnlineTradeRequest
            {
                QuoteKey = quoteKey,
                TPConfirmNo = tpConfirmNo,
            };
        }
    }
}
