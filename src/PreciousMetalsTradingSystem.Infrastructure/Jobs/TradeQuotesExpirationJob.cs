using PreciousMetalsTradingSystem.Application.Common;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.QuoteExpiration;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.Jobs
{
    public class TradeQuotesExpirationJob : BaseJob
    {
        private readonly ILogger<TradeQuotesExpirationJob> _logger;

        public TradeQuotesExpirationJob(IMediator mediator, ILogger<TradeQuotesExpirationJob> logger) 
            : base(mediator)
        {
            _logger = logger;
        }

        [DisableConcurrentExecution(60)]
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TraceContext.TraceID = Guid.NewGuid().ToString();
            _logger.LogInformation($"Trade Quotes Expiration Job started (TraceId: {TraceContext.TraceID})");

            var request = new MarkTradeQuotesAsExpiredCommand();
            await Mediator.Send(request, cancellationToken);

            _logger.LogInformation($"Trade Quotes Expiration Job finished (TraceId: {TraceContext.TraceID})");
        }
    }
}
