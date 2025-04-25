using PreciousMetalsTradingSystem.Application.Trading.Calendar.Exceptions;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Services;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Calendar.Services
{
    public class CalendarServiceTests
    {
        private readonly Mock<IHolidayProvider> _holidayProviderMock;
        private readonly CalendarService _calendarService;

        public CalendarServiceTests()
        {
            _holidayProviderMock = new Mock<IHolidayProvider>();
            _calendarService = new CalendarService(_holidayProviderMock.Object);
        }

        [Theory]
        [InlineData("2024-01-04", 1, "2024-01-05")] // Starts on Thursday, 1 business day -> Friday
        [InlineData("2024-01-04", 2, "2024-01-08")] // Starts on Thursday, spans weekend -> Monday
        [InlineData("2024-07-03", 1, "2024-07-05")] // Starts on Wednesday, skips 4th of July -> Friday
        [InlineData("2024-09-01", 1, "2024-09-03")] // Starts on Sunday, skips weekend & Labor Day -> Tuesday
        [InlineData("2024-12-24", 3, "2024-12-30")] // Starts on Tuesday, skips Christmas -> Monday
        [InlineData("2024-11-28", 3, "2024-12-03")] // Starts on Thanksgiving, spans weekend -> Tuesday
        public async Task AddBusinessDays_ShouldReturnExpectedDate(
            string startDate,
            int businessDaysToAdd,
            string expectedDate)
        {
            // Arrange
            var date = DateOnly.Parse(startDate);
            var expected = DateOnly.Parse(expectedDate);
            CalendarType calendarType = CalendarType.FederalReserve;
            var holidays = new List<Holiday>
            {
                new() { Caption = "New Year's Day", Date = new DateOnly(2024, 1, 1) },
                new() { Caption = "Independence Day", Date = new DateOnly(2024, 7, 4) },
                new() { Caption = "Labor Day", Date = new DateOnly(2024, 9, 2) },
                new() { Caption = "Christmas Day", Date = new DateOnly(2024, 12, 25) },
                new() { Caption = "Thanksgiving Day", Date = new DateOnly(2024, 11, 28) }
            };

            // Mock GetHolidaysAsync to return the specified holidays
            _holidayProviderMock
                .Setup(p => p.GetHolidaysAsync(It.IsAny<int>(), calendarType, It.IsAny<CancellationToken>()))
                .ReturnsAsync(holidays);

            // Act
            var result = await _calendarService.AddBusinessDaysAsync(date, businessDaysToAdd, calendarType);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task AddBusinessDays_ShouldThrowHolidayDataNotFoundException_WhenNoHolidayDataExists()
        {
            // Arrange
            DateOnly startDate = new(2024, 1, 1);
            int daysToAdd = 3;
            CalendarType calendarType = CalendarType.FederalReserve;

            // Mock GetHolidaysAsync to return an empty list, simulating missing holiday data
            _holidayProviderMock
                .Setup(p => p.GetHolidaysAsync(It.IsAny<int>(), calendarType, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Holiday>());

            // Act & Assert
            await Assert.ThrowsAsync<HolidaysNotFoundException>(() =>
                _calendarService.AddBusinessDaysAsync(startDate, daysToAdd, calendarType));
        }

        [Theory]
        [InlineData("2024-01-05", 1, "2024-01-08")] // Starts on Friday, 1 business day over weekend -> Monday
        [InlineData("2024-11-29", 2, "2024-12-03")] // Starts on Black Friday, 2 business days over weekend -> Tuesday
        [InlineData("2024-12-30", 2, "2025-01-02")] // Starts on Monday, spans New Year's holiday -> Thursday
        public async Task AddBusinessDays_ShouldSkipWeekendsAndHolidays_WhenCalculatingBusinessDays(
            string startDate,
            int businessDaysToAdd,
            string expectedDate)
        {
            // Arrange
            var date = DateOnly.Parse(startDate);
            var expected = DateOnly.Parse(expectedDate);
            CalendarType calendarType = CalendarType.FederalReserve;
            var holidays = new List<Holiday>
            {
                new() { Caption = "New Year's Day", Date = new DateOnly(2025, 1, 1) }
            };

            // Mock GetHolidaysAsync to return the specified holidays
            _holidayProviderMock
                .Setup(p => p.GetHolidaysAsync(It.IsAny<int>(), calendarType, It.IsAny<CancellationToken>()))
                .ReturnsAsync(holidays);

            // Act
            var result = await _calendarService.AddBusinessDaysAsync(date, businessDaysToAdd, calendarType);

            // Assert
            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(2025, 4, 19, false)] // Saturday
        [InlineData(2025, 4, 20, false)] // Sunday
        [InlineData(2025, 4, 21, true)]  // Monday
        public async Task IsBusinessDayAsync_ReturnsCorrectValueForWeekends(int year, int month, int day, bool expected)
        {
            // Arrange
            var date = new DateOnly(year, month, day);
            var calendarType = CalendarType.FederalReserve;

            // Act
            var result = await _calendarService.IsBusinessDayAsync(date, calendarType);

            // Assert
            Assert.Equal(expected, result);
            _holidayProviderMock.Verify(x => x.GetHolidaysAsync(year, calendarType, default), Times.AtMostOnce);
        }

        [Fact]
        public async Task IsBusinessDayAsync_ReturnsFalseForHoliday()
        {
            // Arrange
            var date = new DateOnly(2025, 1, 1); // New Year's Day
            var calendarType = CalendarType.FederalReserve;
            var holidays = new List<Holiday>
            {
                new() { Caption = "New Year's Day", Date = new DateOnly(2025, 1, 1) }
            };

            _holidayProviderMock.Setup(x => x.GetHolidaysAsync(2025, calendarType, default))
                .ReturnsAsync(holidays);

            // Act
            var result = await _calendarService.IsBusinessDayAsync(date, calendarType);

            // Assert
            Assert.False(result);
            _holidayProviderMock.Verify(x => x.GetHolidaysAsync(2025, calendarType, default), Times.Once);
        }

        [Fact]
        public async Task IsBusinessDayAsync_ReturnsTrueForNonHolidayBusinessDay()
        {
            // Arrange
            var date = new DateOnly(2025, 4, 21); // Monday, not New Year's Day
            var calendarType = CalendarType.FederalReserve;
            var holidays = new List<Holiday>
            {
                new() { Caption = "New Year's Day", Date = new DateOnly(2025, 1, 1) }
            };

            _holidayProviderMock.Setup(x => x.GetHolidaysAsync(2025, calendarType, default))
                .ReturnsAsync(holidays);

            // Act
            var result = await _calendarService.IsBusinessDayAsync(date, calendarType);

            // Assert
            Assert.True(result);
            _holidayProviderMock.Verify(x => x.GetHolidaysAsync(2025, calendarType, default), Times.Once);
        }

        [Fact]
        public async Task IsBusinessDayAsync_ReturnsTrueForBusinessDayWithNoHolidays()
        {
            // Arrange
            var date = new DateOnly(2025, 4, 21); // Monday
            var calendarType = CalendarType.FederalReserve;
            _holidayProviderMock.Setup(x => x.GetHolidaysAsync(2025, calendarType, default))
                .ReturnsAsync(new List<Holiday>()); // Returns an empty list of holidays

            // Act
            var result = await _calendarService.IsBusinessDayAsync(date, calendarType);

            // Assert
            Assert.True(result);
            _holidayProviderMock.Verify(x => x.GetHolidaysAsync(2025, calendarType, default), Times.Once);
        }
    }
}
