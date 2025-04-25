namespace PreciousMetalsTradingSystem.Domain.Events
{
    /// <summary>
    /// Interface for Domain Events
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// The UTC time when this domain event occurred
        /// </summary>
        DateTime OccurredOnUtc { get; }

        /// <summary>
        /// A unique identifier for the event
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// The type of entity that raised this event
        /// </summary>
        string EventSource { get; }
    }
}
