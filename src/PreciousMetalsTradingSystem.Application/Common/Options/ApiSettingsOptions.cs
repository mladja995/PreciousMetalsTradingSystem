namespace PreciousMetalsTradingSystem.Application.Common.Options
{
    public class ApiSettingsOptions
    {
        public int DefaultPaginationPageSize { get; init; }
        public bool UseMockAMarkTradingService { get; init; }
        public int SpotPricesRefreshCacheFrequencyInMinutes { get; init; }
        public int QuoteValidityPeriodInSeconds { get; init; }
        public int DomainEventsProcessingBatchSize { get; init; }
        public int TradeDuplicateLookupPeriodInDays { get; init; }
        public string TradingClosingHours { get; init; }
    }
}
