using PreciousMetalsTradingSystem.Domain.Events;

namespace PreciousMetalsTradingSystem.Application.Common.DomainEvents
{
    /// <summary>
    /// Interface for queuing domain events for background processing
    /// </summary>
    public interface IDomainEventQueue
    {
        /// <summary>
        /// Enqueues domain events for background processing
        /// </summary>
        /// <param name="domainEvents">The domain events to enqueue</param>
        void EnqueueEvents(IEnumerable<IDomainEvent> domainEvents);

        /// <summary>
        /// Tries to dequeue a domain event for processing
        /// </summary>
        /// <returns>A domain event if one is available, otherwise null</returns>
        IDomainEvent? DequeueEvent();

        /// <summary>
        /// Gets the count of events currently in the queue
        /// </summary>
        int Count { get; }
    }
}
