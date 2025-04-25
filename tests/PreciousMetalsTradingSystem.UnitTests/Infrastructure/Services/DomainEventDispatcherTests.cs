using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Infrastructure.Services
{
    public class DomainEventDispatcherTests
    {
        private readonly Mock<IPublisher> _mediatorMock;
        private readonly Mock<ILogger<DomainEventDispatcher>> _loggerMock;
        private readonly DomainEventDispatcher _dispatcher;

        public DomainEventDispatcherTests()
        {
            _mediatorMock = new Mock<IPublisher>();
            _loggerMock = new Mock<ILogger<DomainEventDispatcher>>();
            _dispatcher = new DomainEventDispatcher(_mediatorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenMediatorIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new DomainEventDispatcher(
                null!,
                _loggerMock.Object));

            Assert.Equal("mediator", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new DomainEventDispatcher(
                _mediatorMock.Object,
                null!));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public async Task DispatchEventsAsync_ShouldDoNothing_WhenDomainEventsIsNull()
        {
            // Arrange
            IEnumerable<IDomainEvent> events = null!;

            // Act
            await _dispatcher.DispatchEventsAsync(events);

            // Assert
            _mediatorMock.Verify(
                m => m.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DispatchEventsAsync_ShouldDoNothing_WhenDomainEventsIsEmpty()
        {
            // Arrange
            var events = new List<IDomainEvent>();

            // Act
            await _dispatcher.DispatchEventsAsync(events);

            // Assert
            _mediatorMock.Verify(
                m => m.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DispatchEventsAsync_ShouldDispatchAllEvents_WhenEventsAreValid()
        {
            // Arrange
            var events = new List<IDomainEvent>
            {
                new TestDomainEvent(),
                new TestDomainEvent()
            };

            // Act
            await _dispatcher.DispatchEventsAsync(events);

            // Assert
            _mediatorMock.Verify(
                m => m.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task DispatchEventsAsync_ShouldContinueProcessing_WhenOneEventFails()
        {
            // Arrange
            var events = new List<IDomainEvent>
            {
                new TestDomainEvent(),
                new TestDomainEvent()
            };

            // Setup the first call to throw an exception
            _mediatorMock
                .SetupSequence(m => m.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"))
                .Returns(Task.CompletedTask);

            // Act
            await _dispatcher.DispatchEventsAsync(events);

            // Assert
            _mediatorMock.Verify(
                m => m.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            // Verify error was logged
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error dispatching domain event")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private record TestDomainEvent()
            : DomainEvent(nameof(TestDomainEvent));
    }
}
