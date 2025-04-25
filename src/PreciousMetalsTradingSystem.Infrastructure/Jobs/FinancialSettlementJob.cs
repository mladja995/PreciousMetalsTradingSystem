using PreciousMetalsTradingSystem.Application.Common;
using PreciousMetalsTradingSystem.Application.Financials.Settlement.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.Jobs
{
    public class FinancialSettlementJob : BaseJob
    {
        private readonly ILogger<FinancialSettlementJob> _logger;

        public FinancialSettlementJob(IMediator mediator, ILogger<FinancialSettlementJob> logger)
            : base(mediator)
        {
            _logger = logger;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TraceContext.TraceID = Guid.NewGuid().ToString();
            _logger.LogInformation($"Financial Settlement job started (TraceId: {TraceContext.TraceID})");

            await Mediator.Send(new FinancialSettlementCommand(), cancellationToken);
            
            _logger.LogInformation($"Financial Settlement job fnished (TraceId: {TraceContext.TraceID})");

        }
    }
}
