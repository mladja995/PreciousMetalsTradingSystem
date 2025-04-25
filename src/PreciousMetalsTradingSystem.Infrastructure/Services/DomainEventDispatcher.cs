using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.Services
{
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IPublisher _mediator;
        private readonly ILogger<DomainEventDispatcher> _logger;

        public DomainEventDispatcher(
            IPublisher mediator,
            ILogger<DomainEventDispatcher> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task DispatchEventsAsync(
            IEnumerable<IDomainEvent> domainEvents, 
            CancellationToken cancellationToken = default)
        {
            if (domainEvents == null || !domainEvents.Any())
            {
                return;
            }

            var domainEventsList = domainEvents.ToList();
            _logger.LogDebug("Dispatching {Count} domain events", domainEventsList.Count);

            foreach (var domainEvent in domainEventsList)
            {
                _logger.LogDebug("Dispatching domain event {EventType} (ID: {EventId})",
                    domainEvent.GetType().Name, domainEvent.EventId);

                try
                {
                    var notification = Activator.CreateInstance(
                        typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType()),
                        domainEvent);

                    await _mediator.Publish(notification!, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error dispatching domain event {EventType} (ID: {EventId})",
                        domainEvent.GetType().Name, domainEvent.EventId);

                    // Note: We're catching the exception but not rethrowing it
                    // This ensures that all events get processed even if one fails
                }
            }
        }
    }
}
