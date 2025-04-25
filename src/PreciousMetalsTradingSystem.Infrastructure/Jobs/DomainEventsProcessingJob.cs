using PreciousMetalsTradingSystem.Application.Common;
using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.Jobs
{
    public class DomainEventsProcessingJob : BaseJob
    {
        private readonly ILogger<ConfirmTradesJob> _logger;

        public DomainEventsProcessingJob(IMediator mediator, ILogger<ConfirmTradesJob> logger)
            : base(mediator)
        {
            _logger = logger;
        }

        [DisableConcurrentExecution(60)]
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TraceContext.TraceID = Guid.NewGuid().ToString();
            _logger.LogInformation($"Domain Events Processing Job started (TraceId: {TraceContext.TraceID})");

            var request = new ProcessPendingDomainEventsCommand();
            await Mediator.Send(request, cancellationToken);

            _logger.LogInformation($"Domain Events Processing Job finished (TraceId: {TraceContext.TraceID})");
        }
    }
}
