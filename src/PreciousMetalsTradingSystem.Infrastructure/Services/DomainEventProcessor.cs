using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.Services
{
    /// <summary>
    /// Service for processing queued domain events
    /// </summary>
    public class DomainEventProcessor : IDomainEventProcessor
    {
        private readonly IDomainEventQueue _domainEventQueue;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ILogger<DomainEventProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the DomainEventProcessor
        /// </summary>
        /// <param name="domainEventQueue">The queue containing domain events</param>
        /// <param name="domainEventDispatcher">The dispatcher for domain events</param>
        /// <param name="logger">Logger for this processor</param>
        public DomainEventProcessor(
            IDomainEventQueue domainEventQueue,
            IDomainEventDispatcher domainEventDispatcher,
            ILogger<DomainEventProcessor> logger)
        {
            _domainEventQueue = domainEventQueue;
            _domainEventDispatcher = domainEventDispatcher;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<int> ProcessEventsAsync(int batchSize, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting to process up to {BatchSize} domain events", batchSize);

            int processedCount = 0;

            for (int i = 0; i < batchSize; i++)
            {
                // Check if there are any events to process
                var domainEvent = _domainEventQueue.DequeueEvent();
                if (domainEvent == null)
                {
                    break;
                }

                try
                {
                    // Process the event
                    await _domainEventDispatcher.DispatchEventsAsync([domainEvent], cancellationToken);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing domain event {EventType} (ID: {EventId})",
                        domainEvent.GetType().Name, domainEvent.EventId);

                    // Since we're dealing with non-critical events,
                    // we can just log the error and continue
                }

                // Check for cancellation
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("Processed {ProcessedCount} domain events. Remaining in queue: {RemainingCount}",
                processedCount, _domainEventQueue.Count);

            return processedCount;
        }
    }
}
