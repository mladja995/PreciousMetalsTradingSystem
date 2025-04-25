using PreciousMetalsTradingSystem.Domain.Events;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Common.DomainEvents
{
    /// <summary>
    /// A wrapper class that adapts domain events to MediatR notifications
    /// </summary>
    /// <typeparam name="T">The type of domain event being wrapped</typeparam>
    public class DomainEventNotification<T> : INotification where T : IDomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the DomainEventNotification class
        /// </summary>
        /// <param name="domainEvent">The domain event to wrap</param>
        public DomainEventNotification(T domainEvent)
        {
            DomainEvent = domainEvent;
        }

        /// <summary>
        /// Gets the wrapped domain event
        /// </summary>
        public T DomainEvent { get; }
    }
}
