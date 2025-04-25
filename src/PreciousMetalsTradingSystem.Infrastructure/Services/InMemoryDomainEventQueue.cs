using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace PreciousMetalsTradingSystem.Infrastructure.Services
{
    /// <summary>
    /// In-memory implementation of IDomainEventQueue
    /// </summary>
    public class InMemoryDomainEventQueue : IDomainEventQueue
    {
        private readonly ConcurrentQueue<IDomainEvent> _queue = new();
        private readonly ILogger<InMemoryDomainEventQueue> _logger;

        public InMemoryDomainEventQueue(ILogger<InMemoryDomainEventQueue> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public int Count => _queue.Count;

        /// <inheritdoc/>
        public void EnqueueEvents(IEnumerable<IDomainEvent> domainEvents)
        {
            if (domainEvents == null || !domainEvents.Any())
            {
                return;
            }

            var eventsList = domainEvents.ToList();

            // Sort events by occurrence time before adding to queue
            var sortedEvents = eventsList.OrderBy(e => e.OccurredOnUtc).ToList();

            foreach (var domainEvent in sortedEvents)
            {
                _queue.Enqueue(domainEvent);
            }

            _logger.LogInformation("Enqueued {Count} domain events for background processing. Queue size: {QueueSize}",
                eventsList.Count, _queue.Count);
        }

        /// <inheritdoc/>
        public IDomainEvent? DequeueEvent()
        {
            if (_queue.TryDequeue(out var domainEvent))
            {
                _logger.LogDebug("Dequeued domain event {EventType} (ID: {EventId}) for processing. Remaining queue size: {QueueSize}",
                    domainEvent.GetType().Name, domainEvent.EventId, _queue.Count);
                return domainEvent;
            }

            return null;
        }
    }
}
