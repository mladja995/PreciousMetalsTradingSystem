
namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Base record for all domain events in the system
    /// </summary>
    public abstract record DomainEvent : IDomainEvent
    {
        /// <summary>
        /// The unique identifier of the event
        /// </summary>
        public Guid EventId { get; }

        /// <summary>
        /// The UTC time when this domain event occurred
        /// </summary>
        public DateTime OccurredOnUtc { get; }

        /// <summary>
        /// The type of entity that raised this event
        /// </summary>
        public string EventSource { get; }

        /// <summary>
        /// Initializes a new instance of the domain event
        /// </summary>
        /// <param name="eventSource">The type of entity that raised this event</param>
        protected DomainEvent(string eventSource)
        {
            EventId = Guid.NewGuid();
            OccurredOnUtc = DateTime.UtcNow;
            EventSource = eventSource;
        }
    }
}