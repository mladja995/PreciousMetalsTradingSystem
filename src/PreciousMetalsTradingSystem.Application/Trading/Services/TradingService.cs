using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Common.Options;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Services;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreciousMetalsTradingSystem.Application.Trading.Services
{
    public class TradingService : ITradingService
    {
        private readonly ICalendarService _calendarService;
        private readonly ApiSettingsOptions _apiSettingsOptions;

        public TradingService(ICalendarService calendarService,
            IOptions<ApiSettingsOptions> options)
        {
            _calendarService = calendarService;
            _apiSettingsOptions = options.Value;
        }

        public async Task<DateOnly> CalculateFinancialSettlementDateAsync(DateTime tradeDateTimeEST, CalendarType calendarType, CancellationToken cancellationToken = default)
        {
            // Parse the configured trading closing time
            var closingTimeString = _apiSettingsOptions.TradingClosingHours;
            if (!TimeOnly.TryParse(closingTimeString, out var closingTime))
            {
                throw new ArgumentException($"Invalid trading hours format: {closingTimeString}");
            }

            var tradeDate = DateOnly.FromDateTime(tradeDateTimeEST);
            var tradeDateClosingTimeEST = new DateTime(
                tradeDate.Year,
                tradeDate.Month,
                tradeDate.Day,
                closingTime.Hour,
                closingTime.Minute,
                closingTime.Second,
                DateTimeKind.Unspecified);

            // Check if the trade date is a business day
            var isBusinessDay = await _calendarService.IsBusinessDayAsync(tradeDate, calendarType, cancellationToken);

            // Determine if the trade was made before or after closing time
            var businessDaysToAdd = isBusinessDay && tradeDateClosingTimeEST > tradeDateTimeEST ? 2 : 3;
            var settlementDate = await _calendarService.AddBusinessDaysAsync(
                tradeDate,
                businessDaysToAdd,
                calendarType,
                cancellationToken);

            return settlementDate;
        }
    }
}
