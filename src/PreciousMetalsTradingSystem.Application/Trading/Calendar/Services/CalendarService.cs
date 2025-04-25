using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Exceptions;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Trading.Calendar.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly IHolidayProvider _holidayProvider;

        public CalendarService(IHolidayProvider holidayProvider)
        {
            _holidayProvider = holidayProvider;
        }

        public async Task<DateOnly> AddBusinessDaysAsync(
            DateOnly date,
            int days,
            CalendarType calendarType,
            CancellationToken cancellationToken = default)
        {
            var holidayDates = new HashSet<DateOnly>();

            int addedDays = 0;
            while (addedDays < Math.Abs(days))
            {
                date = date.AddDays(Math.Sign(days));

                if (date.IsWeekend())
                    continue;

                if (!holidayDates.Any(x => x.Year == date.Year))
                {
                    var holidays = await _holidayProvider.GetHolidaysAsync(date.Year, calendarType, cancellationToken);
                    
                    holidays.Throw(() => new HolidaysNotFoundException(date.Year, calendarType)).IfEmpty();

                    holidayDates = holidays.Select(h => h.Date).ToHashSet();
                }

                if (date.IsHoliday(holidayDates))
                    continue;

                addedDays++;
            }

            return date;
        }

        public async Task<bool> IsBusinessDayAsync(DateOnly date, CalendarType calendarType, CancellationToken cancellationToken = default)
        {
            var holidayDates = new HashSet<DateOnly>();
            if (date.IsWeekend())
                return false;

            var holidays = await _holidayProvider.GetHolidaysAsync(date.Year, calendarType, cancellationToken);

            holidayDates = holidays.Select(h => h.Date).ToHashSet();

            if (date.IsHoliday(holidayDates))
                return false;

            return true;
        }
    }
}
