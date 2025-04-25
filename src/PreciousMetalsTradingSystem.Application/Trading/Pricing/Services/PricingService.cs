using PreciousMetalsTradingSystem.Application.AMark.Models;
using PreciousMetalsTradingSystem.Application.AMark.Services;
using PreciousMetalsTradingSystem.Application.Caching;
using PreciousMetalsTradingSystem.Application.Common.Options;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Services;
using PreciousMetalsTradingSystem.Application.Trading.Pricing.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using Microsoft.Extensions.Options;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Common.Models;

namespace PreciousMetalsTradingSystem.Application.Trading.Pricing.Services
{
    public class PricingService : IPricingService
    {
        private const string LAST_CACHED_TIME_CACHE_KEY = "raw_spot_prices_last_cached_time";
        private const string SPOT_PRICES_CACHE_KEY = "raw_spot_prices_items";

        private readonly ApiSettingsOptions _options;
        private readonly IAMarkTradingServiceFactory _factory;
        private readonly IProductsService _service;
        private readonly ICacheService _cacheService;

        public PricingService(
            IOptions<ApiSettingsOptions> options, 
            IAMarkTradingServiceFactory factory,
            IProductsService service,
            ICacheService cacheService)
        {
            _options = options.Value;
            _factory = factory;
            _service = service;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<ProductPrice>> GetSpotPricesAsync(
          LocationType? location,
          SideType? side,
          CancellationToken cancellationToken = default)
        {
            var allSpotPrices = new List<ProductPrice>();
            var locationTypes = GetLocations(location);

            foreach (var locationType in locationTypes)
            {
                var sideTypes = GetSides(side);
                foreach (var sideType in sideTypes)
                {
                    var (lastTimeCached, rawSpotPrices) = await GetCachedRawSpotPrices(locationType, sideType, cancellationToken);

                    if (IsCacheEmptyOrStale(rawSpotPrices, lastTimeCached))
                    {
                        rawSpotPrices = await FetchRawSpotPrices(locationType, sideType, cancellationToken);

                        lastTimeCached = await CacheRawSpotPrices(rawSpotPrices, locationType, sideType, cancellationToken);
                    }
                    // TODO: Refactor -> Do NOT make call to db for each iteration, load all products with their configurations before building spot prices
                    var spotPrices = (await BuildSpotPrices(
                            locationType,
                            sideType,
                            rawSpotPrices!,
                            lastTimeCached!.Value,
                            cancellationToken)).ToList();

                    allSpotPrices.AddRange(spotPrices);
                }
            }

            return allSpotPrices;
        }

        private IEnumerable<LocationType> GetLocations(LocationType? location)
            => location.HasValue ? 
                [location.Value]
                :  Enum.GetValues(typeof(LocationType)).Cast<LocationType>();

        private IEnumerable<SideType> GetSides(SideType? sideType)
           => sideType.HasValue ?
               [sideType.Value]
               : Enum.GetValues(typeof(SideType)).Cast<SideType>();

        private async Task<DateTime> CacheRawSpotPrices(
            QuoteResponse rawSpotPrices, 
            LocationType location,
            SideType side,
            CancellationToken cancellationToken)
        {
            var timestampUtc = DateTime.UtcNow;

            await _cacheService.SetAsync(
                $"{SPOT_PRICES_CACHE_KEY}_{location}_{side}", 
                rawSpotPrices, 
                cancellationToken);
            
            await _cacheService.SetAsync(
                $"{LAST_CACHED_TIME_CACHE_KEY}_{location}_{side}", 
                new DateTimeWrapper { Value = timestampUtc }, 
                cancellationToken);

            return timestampUtc;
        }

        private async Task<(DateTime?, QuoteResponse?)> GetCachedRawSpotPrices(
            LocationType location, 
            SideType side, 
            CancellationToken cancellationToken)
        {
            var spotPrices = await _cacheService.GetAsync<QuoteResponse>(
                $"{SPOT_PRICES_CACHE_KEY}_{location}_{side}",
                cancellationToken);

            var lastCachedTimeWrapper = await _cacheService.GetAsync<DateTimeWrapper>(
                $"{LAST_CACHED_TIME_CACHE_KEY}_{location}_{side}", cancellationToken);

            DateTime? lastCachedTime = lastCachedTimeWrapper?.Value;

            return new(lastCachedTime, spotPrices);
        }

        private async Task<QuoteResponse> FetchRawSpotPrices(
            LocationType location,
            SideType side,
            CancellationToken cancellationToken)    
        {
            var service = await _factory.CreateAsync(location, cancellationToken);

            var quoteResponse = await service.RequestOnlineQuoteAsync(
                BuildOnlineQuoteRequest(side), 
                cancellationToken);

            return quoteResponse;
        }

        private static OnlineQuoteRequest BuildOnlineQuoteRequest(SideType side)
        {
            return new OnlineQuoteRequest
            {
                OrderType = side.ToAMarkOrderType(),
                ProductQuoteItems = Enum.GetValues(typeof(MetalType))
                    .Cast<MetalType>()
                    .Select(metal => new ProductQuoteItem
                    {
                        ProductCode = metal.ToAMarkSpotDeferredProductCode(),
                        ProductQuantity = 1m
                    }).ToList()
            };
        }

        private async Task<IEnumerable<ProductPrice>> BuildSpotPrices(
            LocationType location,
            SideType side, 
            QuoteResponse rawSpotPrices,
            DateTime timestamptUtc,
            CancellationToken cancellationToken)
        {

            var products = await _service.GetAvailableProducts(location, cancellationToken);

            return products
                .Select(p => new ProductPrice
                {
                    ProductSKU = p.SKU.Value,
                    ProductName = p.Name,
                    Location = location,
                    Side = side.ToClientSideType(),
                    IsAvaiable = p.IsAvailableForTrading(location, side),
                    TimestampUtc = timestamptUtc, 
                    WeightInOz = p.WeightInOz.Value,
                    PremiumUnitType = p.GetPremiumUnitType(location)!.Value,
                    PremiumPerOz = p.GetPremium(location, side)!.Value,
                    SpotPricePerOz = rawSpotPrices
                        .QuoteProductsPricingList
                        .Single(x => p.MetalType.ToAMarkSpotDeferredProductCode() == x.ProductCode)
                        .SpotPrice
                });
        }

        private bool IsCacheEmptyOrStale(QuoteResponse? rawSpotPrices, DateTime? lastCachedTime) =>
            rawSpotPrices == null ||
            lastCachedTime == null ||
            lastCachedTime.Value < DateTime.UtcNow.AddMinutes(-_options.SpotPricesRefreshCacheFrequencyInMinutes);
    }
}
