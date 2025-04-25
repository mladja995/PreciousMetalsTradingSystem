using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Common.Options;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Services;
using PreciousMetalsTradingSystem.Application.Trading.Services;
using Microsoft.Extensions.Options;
using Moq;
using System.Globalization;
using Xunit;

namespace PreciousMetalsTradingSystem.Application.Trading.Tests.Services
{
    public class TradingServiceTests
    {
        private readonly ICalendarService _calendarService;
        private readonly Mock<IOptions<ApiSettingsOptions>> _apiSettingsOptionsMock;
        private readonly TradingService _tradingService;
        private readonly StaticHolidayProvider _staticHolidayProviderMock;

        public TradingServiceTests()
        {
            _staticHolidayProviderMock = new StaticHolidayProvider();
            _calendarService = new CalendarService(_staticHolidayProviderMock);
            _apiSettingsOptionsMock = new Mock<IOptions<ApiSettingsOptions>>();
            

            // Setting a default value for TradingClosingHours
            _apiSettingsOptionsMock.Setup(options => options.Value).Returns(new ApiSettingsOptions { TradingClosingHours = "16:00pm" });
            _tradingService = new TradingService(_calendarService, _apiSettingsOptionsMock.Object);
        }

        [Theory]
        [InlineData("2025-04-21", "10:00:59", "2025-04-23")] // Monday -> Wednesday
        [InlineData("2025-04-23", "09:00:59", "2025-04-25")] // Wednesday -> Friday
        [InlineData("2025-04-24", "15:59:59", "2025-04-28")] // Thursday -> Monday (skips weekend)
        [InlineData("2025-04-25", "11:00:59", "2025-04-29")] // Friday -> Tuesday (skips weekend)
        public async Task CalculateFinancialSettlementDateAsync_TradeBeforeClosingOnBusinessDay_ReturnsDatePlusTwoBusinessDays(string tradeDateString, string tradeTimeString,
        string expectedSettlementDateString)
        {
            // Arrange
            var tradeDateTimeEST = DateTime.Parse($"{tradeDateString} {tradeTimeString}");
            var calendarType = CalendarType.FederalReserve;
            var expectedSettlementDate = DateOnly.Parse(expectedSettlementDateString); 

            // Act
            var settlementDate = await _tradingService.CalculateFinancialSettlementDateAsync(tradeDateTimeEST, calendarType);

            // Assert
            Assert.Equal(expectedSettlementDate, settlementDate);
        }

        [Theory]
        [InlineData("2025-5-26", "14:00:00", "2025-05-29")] // Monday (Memorial Day 2025) at 10:00 AM -> Thursday
        [InlineData("2024-6-19", "10:00:00", "2024-06-24")] // Wednesday (Juneteenth 2024) at 10:00 AM -> Monday
        [InlineData("2025-6-19", "15:30:00", "2025-06-24")] // Thursday (Juneteenth 2025) at 3:30 PM -> Tuesday
        public async Task CalculateFinancialSettlementDateAsync_TradeDuringBusinessHoursOnWeekdayHoliday_ReturnsDatePlusThreeBusinessDays(
        string tradeDateString,
        string tradeTimeString,
        string expectedSettlementDateString)
        {
            // Arrange
            var tradeDateTimeEST = DateTime.Parse($"{tradeDateString} {tradeTimeString}");
            var calendarType = CalendarType.FederalReserve;
            var expectedSettlementDate = DateOnly.Parse(expectedSettlementDateString);

            // Act
            var settlementDate = await _tradingService.CalculateFinancialSettlementDateAsync(tradeDateTimeEST, calendarType);

            // Assert
            Assert.Equal(expectedSettlementDate, settlementDate);
        }

        [Theory]
        [InlineData("2025-04-21", "16:00:01", "2025-04-24")] // Monday at 16:01:05 -> Thursday
        [InlineData("2025-04-22", "17:00:30", "2025-04-25")] // Tuesday at 17:00:30 -> Friday
        [InlineData("2025-04-23", "20:00:15", "2025-04-28")] // Wednesday at 20:00:15 -> Monday (skips weekend)
        [InlineData("2025-04-24", "18:30:45", "2025-04-29")] // Thursday at 18:30:45 -> Tuesday (skips weekend)
        [InlineData("2025-04-25", "23:59:59", "2025-04-30")] // Friday at 23:59:59 -> Wednesday (skips weekend)
        public async Task CalculateFinancialSettlementDateAsync_TradeAfterClosingOnBusinessDay_ReturnsDatePlusThreeBusinessDays(
        string tradeDateString,
        string tradeTimeString,
        string expectedSettlementDateString)
        {
            // Arrange
            var tradeDateTimeEST = DateTime.Parse($"{tradeDateString} {tradeTimeString}");
            var calendarType = CalendarType.FederalReserve;
            var expectedSettlementDate = DateOnly.Parse(expectedSettlementDateString);
            
            // Act
            var settlementDate = await _tradingService.CalculateFinancialSettlementDateAsync(tradeDateTimeEST, calendarType);

            // Assert
            Assert.Equal(expectedSettlementDate, settlementDate);
        }
        [Theory]
        [InlineData("2025-04-19", "10:00:30", "2025-04-23")] // Saturday morning -> Wednesday
        [InlineData("2025-04-20", "09:00:45", "2025-04-23")] // Sunday -> Wednesday
        [InlineData("2025-04-26", "11:00:20", "2025-04-30")] // Next Saturday -> Wednesday
        [InlineData("2025-04-27", "14:00:50", "2025-04-30")] // Next Sunday -> Wednesday
        [InlineData("2025-05-26", "11:00:00", "2025-05-29")] // Memorial Day (Monday - assumed holiday) -> Thursday
        public async Task CalculateFinancialSettlementDateAsync_TradeBeforeClosingOnNonBusinessDay_ReturnsDatePlusThreeBusinessDays(
           string tradeDateString,
           string tradeTimeString,
           string expectedSettlementDateString)
        {
            // Arrange
            var tradeDateTimeEST = DateTime.Parse($"{tradeDateString} {tradeTimeString}");
            var calendarType = CalendarType.FederalReserve;
            var expectedSettlementDate = DateOnly.Parse(expectedSettlementDateString);

            // Act
            var settlementDate = await _tradingService.CalculateFinancialSettlementDateAsync(tradeDateTimeEST, calendarType);

            // Assert
            Assert.Equal(expectedSettlementDate, settlementDate);
        }

        [Theory]
        [InlineData("2025-04-19", "17:00:00", "2025-04-23")] // Saturday after 4:00 PM EST -> Wednesday
        [InlineData("2025-04-20", "20:00:00", "2025-04-23")] // Sunday after 4:00 PM EST -> Wednesday
        [InlineData("2025-04-26", "18:00:00", "2025-04-30")] // Next Saturday after 4:00 PM EST -> Wednesday
        [InlineData("2025-04-27", "22:00:00", "2025-04-30")] // Next Sunday after 4:00 PM EST -> Wednesday
        [InlineData("2025-05-26", "17:00:00", "2025-05-29")] // Memorial Day (Monday - assumed holiday) after 4:00 PM EST -> Thursday
        public async Task CalculateFinancialSettlementDateAsync_TradeAfterClosingOnNonBusinessDay_ReturnsDatePlusThreeBusinessDays(
        string tradeDateString,
        string tradeTimeString,
        string expectedSettlementDateString)
        {
            // Arrange
            var tradeDateTimeEST = DateTime.Parse($"{tradeDateString} {tradeTimeString}", CultureInfo.InvariantCulture);
            var calendarType = CalendarType.FederalReserve;
            var expectedSettlementDate = DateOnly.Parse(expectedSettlementDateString);

            // Act
            var settlementDate = await _tradingService.CalculateFinancialSettlementDateAsync(tradeDateTimeEST, calendarType);

            // Assert
            Assert.Equal(expectedSettlementDate, settlementDate);
        }

        [Fact]
        public async Task CalculateFinancialSettlementDateAsync_TradeAtClosingTimeOnBusinessDay_ReturnsDatePlusThreeBusinessDays()
        {
            // Arrange
            var tradeDateTimeEST = new DateTime(2025, 04, 23, 16, 0, 0); // Wednesday exactly at 4:00 PM EST
            var calendarType = CalendarType.FederalReserve;
            var expectedSettlementDate = new DateOnly(2025, 04, 28); // Monday (Wednesday + 3 business days, skipping weekend)

            // Act
            var settlementDate = await _tradingService.CalculateFinancialSettlementDateAsync(tradeDateTimeEST, calendarType);

            // Assert
            Assert.Equal(expectedSettlementDate, settlementDate);
        }
    }
}