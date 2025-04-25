using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace PreciousMetalsTradingSystem.Application.Common.Behaviors
{
    public class LoggingPipelineBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

        public LoggingPipelineBehavior(
            ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request, 
            RequestHandlerDelegate<TResponse> next, 
            CancellationToken cancellationToken)
        {
            var traceId = TraceContext.TraceID;

            _logger.LogInformation(
                "Starting request {@RequestName}, {@TraceID}, {@DateTimeUtc}",
                typeof(TRequest).Name,
                traceId,
                DateTime.UtcNow);

            var stopwatch = Stopwatch.StartNew();
            
            var result = await next();
            
            stopwatch.Stop();

            _logger.LogInformation(
                "Completed request {RequestName} (TraceID: {TraceID}) at {DateTimeUtc}, took {ElapsedMilliseconds} ms",
                typeof(TRequest).Name,
                traceId,
                DateTime.UtcNow,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
    }
}
