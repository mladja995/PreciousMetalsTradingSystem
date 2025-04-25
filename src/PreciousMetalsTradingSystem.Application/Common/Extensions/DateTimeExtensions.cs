namespace PreciousMetalsTradingSystem.Application.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ConvertUtcToEst(this DateTime date)
        {
            var estTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(date, estTimeZone);
        }


        public static DateOnly ConvertUtcToEstDateOnly(this DateTime date)
        {
            var estTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(date, estTimeZone));
        }

        public static DateTime ConvertEstToUtc(this DateTime date)
        {
            var estTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            return TimeZoneInfo.ConvertTimeToUtc(date, estTimeZone);
        }
    }
}
