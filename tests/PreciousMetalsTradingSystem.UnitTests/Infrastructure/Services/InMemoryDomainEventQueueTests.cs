using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Infrastructure.Services
{
    public class InMemoryDomainEventQueueTests
    {
        private readonly Mock<ILogger<InMemoryDomainEventQueue>> _loggerMock;
        private readonly InMemoryDomainEventQueue _queue;

        public InMemoryDomainEventQueueTests()
        {
            _loggerMock = new Mock<ILogger<InMemoryDomainEventQueue>>();
            _queue = new InMemoryDomainEventQueue(_loggerMock.Object);
        }

        [Fact]
        public void EnqueueEvents_ShouldAddEventsToQueue()
        {
            // Arrange
            var events = new List<IDomainEvent>
            {
                new TestDomainEvent(),
                new TestDomainEvent(),
                new TestDomainEvent()
            };

            // Act
            _queue.EnqueueEvents(events);

            // Assert
            Assert.Equal(3, _queue.Count);
        }

        [Fact]
        public void EnqueueEvents_ShouldSortEventsByOccurrenceTime()
        {
            // Arrange

            // Create events with different timestamps
            var event1 = new TestDomainEvent();
            var event2 = new TestDomainEvent();
            var event3 = new TestDomainEvent();

            var events = new List<IDomainEvent> { event2, event1, event3 };

            // Act
            _queue.EnqueueEvents(events);

            // Assert
            Assert.Equal(3, _queue.Count);

            // Verify events are dequeued in chronological order
            var firstDequeued = _queue.DequeueEvent();
            var secondDequeued = _queue.DequeueEvent();
            var thirdDequeued = _queue.DequeueEvent();

            // Check they come out in timestamp order
            Assert.Equal(event1, firstDequeued);
            Assert.Equal(event2, secondDequeued);
            Assert.Equal(event3, thirdDequeued);
        }

        [Fact]
        public void DequeueEvent_ShouldReturnNull_WhenQueueIsEmpty()
        {
            // Act
            var result = _queue.DequeueEvent();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DequeueEvent_ShouldReturnAndRemoveEvents_InFIFOOrder()
        {
            // Arrange
            var event1 = new TestDomainEvent();
            var event2 = new TestDomainEvent();
            var event3 = new TestDomainEvent();

            var events = new List<IDomainEvent> { event1, event2, event3 };

            _queue.EnqueueEvents(events);
            Assert.Equal(3, _queue.Count);

            // Act & Assert - Events should come out in chronological order
            var firstDequeued = _queue.DequeueEvent();
            Assert.Equal(event1.EventId, firstDequeued!.EventId);
            Assert.Equal(2, _queue.Count);

            var secondDequeued = _queue.DequeueEvent();
            Assert.Equal(event2.EventId, secondDequeued!.EventId);
            Assert.Equal(1, _queue.Count);

            var thirdDequeued = _queue.DequeueEvent();
            Assert.Equal(event3.EventId, thirdDequeued!.EventId);
            Assert.Equal(0, _queue.Count);

            // Should be empty now
            var emptyResult = _queue.DequeueEvent();
            Assert.Null(emptyResult);
        }

        [Fact]
        public void EnqueueEvents_ShouldAppendToExistingQueue()
        {
            // Arrange
            var firstEvents = new List<IDomainEvent>
            {
                new TestDomainEvent()
            };

            var secondEvents = new List<IDomainEvent>
            {
                new TestDomainEvent(),
                new TestDomainEvent()
            };

            // Act
            _queue.EnqueueEvents(firstEvents);
            Assert.Equal(1, _queue.Count);

            _queue.EnqueueEvents(secondEvents);

            // Assert
            Assert.Equal(3, _queue.Count);
        }

        private record TestDomainEvent()
            : DomainEvent(nameof(TestDomainEvent));
    }
}
