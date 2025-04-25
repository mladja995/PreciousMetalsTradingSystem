using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;

namespace PreciousMetalsTradingSystem.Application.Trading.Calendar.Services
{
    public interface IHolidayProvider
    {
        Task<IEnumerable<Holiday>> GetHolidaysAsync(
            int year,
            CalendarType calendarType,
            CancellationToken cancellationToken = default);
    }
}
