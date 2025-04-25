namespace PreciousMetalsTradingSystem.Application.Common.DomainEvents
{
    /// <summary>
    /// Interface for processing queued domain events
    /// </summary>
    public interface IDomainEventProcessor
    {
        /// <summary>
        /// Processes a batch of domain events from the queue
        /// </summary>
        /// <param name="batchSize">Maximum number of events to process in this batch</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of events processed</returns>
        Task<int> ProcessEventsAsync(int batchSize, CancellationToken cancellationToken);
    }
}
