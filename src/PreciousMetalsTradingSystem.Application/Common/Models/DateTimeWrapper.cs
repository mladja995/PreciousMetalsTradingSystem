namespace PreciousMetalsTradingSystem.Application.Common.Models
{
    public class DateTimeWrapper
    {
        public DateTime Value { get; init; }

        public static implicit operator DateTime(DateTimeWrapper x) => x.Value;
    }
}
