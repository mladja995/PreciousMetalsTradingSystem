namespace PreciousMetalsTradingSystem.Infrastructure.Jobs.Options
{
    public class HangfireOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string FinancialSettlementJobCronExpression { get; set; } = string.Empty;
        public string ConfirmTradesJobCronExpression { get; set; } = string.Empty;
        public string TradeQuotesExpirationJobCronExpression { get; set; } = string.Empty;
        public string DomainEventsProcessingJobCronExpression { get; set; } = string.Empty;
    }
}
