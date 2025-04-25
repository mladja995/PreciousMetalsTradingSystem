using PreciousMetalsTradingSystem.Application.Common.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace PreciousMetalsTradingSystem.Application.Common.DomainEvents
{
    internal class ProcessPendingDomainEventsCommandHandler : IRequestHandler<ProcessPendingDomainEventsCommand>
    {
        private readonly IDomainEventProcessor _domainEventProcessor;
        private readonly ApiSettingsOptions _apiSettingsOptions;


        public ProcessPendingDomainEventsCommandHandler(
            IDomainEventProcessor domainEventProcessor, 
            IOptions<ApiSettingsOptions> apiSettingsOptions)
        {
            _domainEventProcessor = domainEventProcessor;
            _apiSettingsOptions = apiSettingsOptions.Value;
        }

        public async Task Handle(ProcessPendingDomainEventsCommand request, CancellationToken cancellationToken)
        {
            await _domainEventProcessor.ProcessEventsAsync(
                _apiSettingsOptions.DomainEventsProcessingBatchSize, cancellationToken);
        }
    }
}
