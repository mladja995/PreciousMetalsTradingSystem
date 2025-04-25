using PreciousMetalsTradingSystem.Application.AMark.Models;
using PreciousMetalsTradingSystem.Application.AMark.Options;
using PreciousMetalsTradingSystem.Application.Common;
using Throw;

namespace PreciousMetalsTradingSystem.Application.AMark.Services
{
    public class AMarkTradingServiceMock : IAMarkTradingService
    {
        private const string COMMODITY_GOLD = "GOLD";
        private const string COMMODITY_SILVER = "SILVER";

        private static readonly Random _random = new();
        private static readonly string[] AllowedProductCodes = [
            Constants.AMARK_SPOT_DEFERRED_TRADE_PRODUCT_CODE_SILVER, 
            Constants.AMARK_SPOT_DEFERRED_TRADE_PRODUCT_CODE_GOLD];
        private static readonly string[] AllowedOrderTypes = [
            Constants.AMARK_ORDER_TYPE_BUY, 
            Constants.AMARK_ORDER_TYPE_SELL];

        private string TicketNumber => $"MOCK-P{_random.Next(10000, 99999)}";
        private decimal GoldSellPrice => GetRandomPrice(2700.00m, 2750.00m);  // Price when we sell
        private decimal GoldBuyPrice => GetRandomPrice(2680.00m, 2730.00m);   // Price when we buy
        private decimal SilverSellPrice => GetRandomPrice(31.50m, 32.50m);    // Price when we sell
        private decimal SilverBuyPrice => GetRandomPrice(31.00m, 32.00m);     // Price when we buy

        private decimal GetRandomPrice(decimal min, decimal max)
        {
            double range = (double)(max - min);
            var price = min + (decimal)(_random.NextDouble() * range);
            return Math.Round(price, 2);
        }

        public async Task<QuoteResponse> RequestOnlineQuoteAsync(
            OnlineQuoteRequest request, 
            CancellationToken cancellationToken = default)
        {
            AllowedOrderTypes.Throw().IfNotContains(request.OrderType);
            request.ProductQuoteItems.ForEach(item => AllowedProductCodes.Throw().IfNotContains(item.ProductCode));

            return await Task.FromResult(new QuoteResponse
            {
                QuoteKey = Guid.NewGuid().ToString(),
                QuoteProductsPricingList = request
                    .ProductQuoteItems
                    .GroupBy(item => item.ProductCode)
                    .Select(group => new QuoteProductPricing
                    {
                        ProductCode = group.Key,
                        CommodityDesc = group.Key == Constants.AMARK_SPOT_DEFERRED_TRADE_PRODUCT_CODE_GOLD 
                            ? COMMODITY_GOLD 
                            : COMMODITY_SILVER,
                        SpotPrice = group.Key == Constants.AMARK_SPOT_DEFERRED_TRADE_PRODUCT_CODE_GOLD
                            ? (request.OrderType == Constants.AMARK_ORDER_TYPE_BUY ? GoldBuyPrice : GoldSellPrice)
                            : (request.OrderType == Constants.AMARK_ORDER_TYPE_BUY ? SilverBuyPrice : SilverSellPrice)
                    }).ToList()
            });
        }

        public async Task<TradeResponse> RequestOnlineTradeAsync(
            OnlineTradeRequest request, 
            CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new TradeResponse
            {
                QuoteKey = request.QuoteKey,
                TicketNumber = TicketNumber
            });
        }

        public void SetCredentials(HedgingAccountCredential credentials)
        {
            // Note: No need to implement for mock service
        }
    }
}
