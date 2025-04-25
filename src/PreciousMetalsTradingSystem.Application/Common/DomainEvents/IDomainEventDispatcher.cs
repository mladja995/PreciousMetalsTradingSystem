using PreciousMetalsTradingSystem.Domain.Events;

namespace PreciousMetalsTradingSystem.Application.Common.DomainEvents
{
    /// <summary>
    /// Interface for dispatching domain events
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Dispatches all domain events
        /// </summary>
        /// <param name="domainEvents">Collection of domain events to dispatch</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task DispatchEventsAsync(
            IEnumerable<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default);
    }
}
