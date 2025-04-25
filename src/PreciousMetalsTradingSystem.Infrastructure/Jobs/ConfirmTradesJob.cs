using PreciousMetalsTradingSystem.Application.Common;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.ConfirmTrades;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.Jobs
{
    public class ConfirmTradesJob : BaseJob
    {
        private readonly ILogger<ConfirmTradesJob> _logger;

        public ConfirmTradesJob(IMediator mediator, ILogger<ConfirmTradesJob> logger) 
            : base(mediator)
        {
            _logger = logger;
        }

        [DisableConcurrentExecution(60)]
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TraceContext.TraceID = Guid.NewGuid().ToString();
            _logger.LogInformation($"Confirm Trades Job started (TraceId: {TraceContext.TraceID})");

            var request = new ConfirmTradesCommand();
            await Mediator.Send(request, cancellationToken);

            _logger.LogInformation($"Confirm Trades Job finished (TraceId: {TraceContext.TraceID})");
        }
    }
}
