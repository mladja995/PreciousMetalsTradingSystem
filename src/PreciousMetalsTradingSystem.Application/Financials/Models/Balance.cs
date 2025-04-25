namespace PreciousMetalsTradingSystem.Application.Financials.Models
{
    public class Balance
    {
        public required decimal AvailableForTrading { get; init; }
        public required decimal Actual { get; init; }
    }
}
