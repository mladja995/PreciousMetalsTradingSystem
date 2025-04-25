using PreciousMetalsTradingSystem.Domain.Events;

namespace PreciousMetalsTradingSystem.Domain.Primitives.Interfaces
{
    /// <summary>
    /// Interface for aggregate roots in the domain model.
    /// Aggregates are clusters of domain objects that can be treated as a single unit.
    /// </summary>
    public interface IAggregate
    {
        /// <summary>
        /// Gets the collection of domain events that have occurred in this aggregate.
        /// Domain events represent something meaningful that happened in the domain.
        /// </summary>
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

        /// <summary>
        /// Adds a domain event to this aggregate.
        /// The event will be dispatched after the aggregate is persisted.
        /// </summary>
        /// <param name="domainEvent">The domain event to add.</param>
        void AddDomainEvent(IDomainEvent domainEvent);

        /// <summary>
        /// Removes a domain event from this aggregate.
        /// </summary>
        /// <param name="domainEvent">The domain event to remove.</param>
        void RemoveDomainEvent(IDomainEvent domainEvent);

        /// <summary>
        /// Clears all domain events from this aggregate.
        /// This is typically called after the events have been dispatched.
        /// </summary>
        void ClearDomainEvents();
    }
}
