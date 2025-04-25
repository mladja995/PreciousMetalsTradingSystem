using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;

namespace PreciousMetalsTradingSystem.Application.Trading.Calendar.Services
{
    public class StaticHolidayProvider : IHolidayProvider
    {
        private static List<Holiday> previousYearFederalReserve = new List<Holiday>
        {
            new() { Caption = "New Year's Day", Date = new DateOnly(2023, 1, 1) },
            new() { Caption = "New Year's Day (substitute)", Date = new DateOnly(2023, 1, 2) },
            new() { Caption = "Martin Luther King Day", Date = new DateOnly(2023, 1, 16) },
            new() { Caption = "Presidents Day", Date = new DateOnly(2023, 2, 20) },
            new() { Caption = "Memorial Day", Date = new DateOnly(2023, 5, 29) },
            new() { Caption = "Juneteenth", Date = new DateOnly(2023, 6, 19) },
            new() { Caption = "Independence Day", Date = new DateOnly(2023, 7, 4) },
            new() { Caption = "Labor Day", Date = new DateOnly(2023, 9, 4) },
            new() { Caption = "Columbus Day", Date = new DateOnly(2023, 10, 9) },
            new() { Caption = "Veterans Day (substitute)", Date = new DateOnly(2023, 11, 10) },
            new() { Caption = "Veterans Day", Date = new DateOnly(2023, 11, 11) },
            new() { Caption = "Thanksgiving Day", Date = new DateOnly(2023, 11, 23) },
            new() { Caption = "Christmas Day", Date = new DateOnly(2023, 12, 25) },
        };
        private static List<Holiday> thisYearFederalReserve = new List<Holiday>
        {
            new() { Caption = "New Year's Day", Date = new DateOnly(2024, 1, 1) },
            new() { Caption = "Martin Luther King Day", Date = new DateOnly(2024, 1, 15) },
            new() { Caption = "Presidents Day", Date = new DateOnly(2024, 2, 19) },
            new() { Caption = "Memorial Day", Date = new DateOnly(2024, 5, 27) },
            new() { Caption = "Juneteenth", Date = new DateOnly(2024, 6, 19) },
            new() { Caption = "Independence Day", Date = new DateOnly(2024, 7, 4) },
            new() { Caption = "Labor Day", Date = new DateOnly(2024, 9, 2) },
            new() { Caption = "Columbus Day", Date = new DateOnly(2024, 10, 14) },
            new() { Caption = "Veterans Day", Date = new DateOnly(2024, 11, 11) },
            new() { Caption = "Thanksgiving Day", Date = new DateOnly(2024, 11, 28) },
            new() { Caption = "Christmas Day", Date = new DateOnly(2024, 12, 25) },
        };
        private static List<Holiday> nextYearFederalReserve = new List<Holiday>
        {
            new() { Caption = "New Year's Day", Date = new DateOnly(2025, 1, 1) },
            new() { Caption = "Martin Luther King Day", Date = new DateOnly(2025, 1, 20) },
            new() { Caption = "Presidents Day", Date = new DateOnly(2025, 2, 17) },
            new() { Caption = "Memorial Day", Date = new DateOnly(2025, 5, 26) },
            new() { Caption = "Juneteenth", Date = new DateOnly(2025, 6, 19) },
            new() { Caption = "Independence Day", Date = new DateOnly(2025, 7, 4) },
            new() { Caption = "Labor Day", Date = new DateOnly(2025, 9, 1) },
            new() { Caption = "Columbus Day", Date = new DateOnly(2025, 10, 13) },
            new() { Caption = "Veterans Day", Date = new DateOnly(2025, 11, 11) },
            new() { Caption = "Thanksgiving Day", Date = new DateOnly(2025, 11, 27) },
            new() { Caption = "Christmas Day", Date = new DateOnly(2025, 12, 25) },
        };

        private static readonly Dictionary<(int, CalendarType), List<Holiday>> HolidaysByYear = new()
        {
            {
                (previousYearFederalReserve.First().Date.Year, CalendarType.FederalReserve), previousYearFederalReserve
            },
            {
                (thisYearFederalReserve.First().Date.Year, CalendarType.FederalReserve), thisYearFederalReserve
            },
            {
                (nextYearFederalReserve.First().Date.Year, CalendarType.FederalReserve), nextYearFederalReserve
            }
        };

        public async Task<IEnumerable<Holiday>> GetHolidaysAsync(
            int year, 
            CalendarType calendarType, 
            CancellationToken cancellationToken = default)
        {
            if (HolidaysByYear.TryGetValue((year, calendarType), out var holidays))
            {
                return await Task.FromResult(holidays);
            }

            return await Task.FromResult(new List<Holiday>());
        }
    }
}
