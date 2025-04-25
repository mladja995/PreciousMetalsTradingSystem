namespace PreciousMetalsTradingSystem.Application.Trading.Calendar.Models
{
    public class Holiday
    {
        public required string Caption { get; init; }
        public required DateOnly Date { get; init; }
    }
}
