using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Infrastructure.Services
{
    public class DomainEventProcessorTests
    {
        private readonly Mock<IDomainEventQueue> _domainEventQueueMock;
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock;
        private readonly Mock<ILogger<DomainEventProcessor>> _loggerMock;
        private readonly DomainEventProcessor _processor;

        public DomainEventProcessorTests()
        {
            _domainEventQueueMock = new Mock<IDomainEventQueue>();
            _domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();
            _loggerMock = new Mock<ILogger<DomainEventProcessor>>();
            _processor = new DomainEventProcessor(
                _domainEventQueueMock.Object,
                _domainEventDispatcherMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessEventsAsync_ShouldReturn0_WhenQueueIsEmpty()
        {
            // Arrange
            _domainEventQueueMock
                .Setup(q => q.DequeueEvent())
                .Returns((IDomainEvent)null!);

            _domainEventQueueMock
                .Setup(q => q.Count)
                .Returns(0);

            // Act
            var result = await _processor.ProcessEventsAsync(10, CancellationToken.None);

            // Assert
            Assert.Equal(0, result);
            _domainEventDispatcherMock.Verify(
                d => d.DispatchEventsAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessEventsAsync_ShouldProcessAllEvents_WhenBatchSizeIsLargerThanQueueSize()
        {
            // Arrange
            var event1 = new TestDomainEvent();
            var event2 = new TestDomainEvent();

            _domainEventQueueMock
                .SetupSequence(q => q.DequeueEvent())
                .Returns(event1)
                .Returns(event2)
                .Returns((IDomainEvent)null!);

            _domainEventQueueMock
                .Setup(q => q.Count)
                .Returns(0);

            // Act
            var result = await _processor.ProcessEventsAsync(10, CancellationToken.None);

            // Assert
            Assert.Equal(2, result);
            _domainEventDispatcherMock.Verify(
                d => d.DispatchEventsAsync(It.Is<IEnumerable<IDomainEvent>>(e => e.Contains(event1)), It.IsAny<CancellationToken>()),
                Times.Once);
            _domainEventDispatcherMock.Verify(
                d => d.DispatchEventsAsync(It.Is<IEnumerable<IDomainEvent>>(e => e.Contains(event2)), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessEventsAsync_ShouldProcessLimitedEvents_WhenBatchSizeIsReached()
        {
            // Arrange
            const int batchSize = 2;
            var events = new[]
            {
                new TestDomainEvent(),
                new TestDomainEvent(),
                new TestDomainEvent()
            };

            int queueIndex = 0;
            _domainEventQueueMock
                .Setup(q => q.DequeueEvent())
                .Returns(() => queueIndex < events.Length ? events[queueIndex++] : null);

            _domainEventQueueMock
                .Setup(q => q.Count)
                .Returns(() => events.Length - queueIndex);

            // Act
            var result = await _processor.ProcessEventsAsync(batchSize, CancellationToken.None);

            // Assert
            Assert.Equal(batchSize, result);
            _domainEventDispatcherMock.Verify(
                d => d.DispatchEventsAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()),
                Times.Exactly(batchSize));
        }

        [Fact]
        public async Task ProcessEventsAsync_ShouldHandleExceptions_AndContinueProcessing()
        {
            // Arrange
            var event1 = new TestDomainEvent();
            var event2 = new TestDomainEvent();

            _domainEventQueueMock
                .SetupSequence(q => q.DequeueEvent())
                .Returns(event1)
                .Returns(event2)
                .Returns((IDomainEvent)null!);

            _domainEventDispatcherMock
                .Setup(d => d.DispatchEventsAsync(It.Is<IEnumerable<IDomainEvent>>(e => e.Contains(event1)), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            _domainEventQueueMock
                .Setup(q => q.Count)
                .Returns(0);

            // Act
            var result = await _processor.ProcessEventsAsync(10, CancellationToken.None);

            // Assert
            Assert.Equal(1, result); // Both events are dequeued, but only one counted as processed, even if one failed
            _domainEventDispatcherMock.Verify(
                d => d.DispatchEventsAsync(It.Is<IEnumerable<IDomainEvent>>(e => e.Contains(event2)), It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify error was logged
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error processing domain event")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessEventsAsync_ShouldRespectCancellationToken()
        {
            // Arrange
            var event1 = new TestDomainEvent();
            var event2 = new TestDomainEvent();

            _domainEventQueueMock
                .SetupSequence(q => q.DequeueEvent())
                .Returns(event1)
                .Returns(event2);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act
            var result = await _processor.ProcessEventsAsync(10, cancellationTokenSource.Token);

            // Assert
            Assert.Equal(1, result); // Only first event is processed before cancellation
            _domainEventDispatcherMock.Verify(
                d => d.DispatchEventsAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        private record TestDomainEvent()
            : DomainEvent(nameof(TestDomainEvent));
    }
}
