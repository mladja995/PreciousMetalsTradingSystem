using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreciousMetalsTradingSystem.Application.Trading.Services
{
    public interface ITradingService
    {
        Task<DateOnly> CalculateFinancialSettlementDateAsync(
            DateTime tradeDateEST,
            CalendarType calendarType,
            CancellationToken cancellationToken = default);
    }
}
