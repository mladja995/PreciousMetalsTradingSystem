namespace PreciousMetalsTradingSystem.Application.Common.Extensions
{
    public static class DateOnlyExtensions
    {
        public static bool IsWeekend(this DateOnly date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsHoliday(this DateOnly date, HashSet<DateOnly> holidays)
        {
            return holidays.Contains(date);
        }
    }
}
