using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;

namespace PreciousMetalsTradingSystem.Application.Trading.Calendar.Exceptions
{
    public class HolidaysNotFoundException : TradingSystemApplicationException
    {
        public HolidaysNotFoundException(int year, CalendarType calendarType)
            : base($"Holiday data not found for year {year} and calendar type {calendarType}.", "NOT_FOUND")
        {
        }
    }
}
