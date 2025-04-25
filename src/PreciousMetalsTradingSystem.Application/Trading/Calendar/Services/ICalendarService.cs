using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;

namespace PreciousMetalsTradingSystem.Application.Trading.Calendar.Services
{
    public interface ICalendarService
    {
        Task<DateOnly> AddBusinessDaysAsync(
            DateOnly date,
            int days, 
            CalendarType calendarType, 
            CancellationToken cancellationToken = default);

        Task<bool> IsBusinessDayAsync(
            DateOnly date,
            CalendarType calendarType,
            CancellationToken cancellationToken = default);
    }
}
