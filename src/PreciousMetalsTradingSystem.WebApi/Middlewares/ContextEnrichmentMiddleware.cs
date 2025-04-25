using PreciousMetalsTradingSystem.Application.Common;
using PreciousMetalsTradingSystem.WebApi.Common;
using Serilog.Context;

namespace PreciousMetalsTradingSystem.WebApi.Middlewares
{
    public class ContextEnrichmentMiddleware
    {
        private readonly RequestDelegate _next;

        public ContextEnrichmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            TraceContext.TraceID = Guid.NewGuid().ToString();

            context.Response.Headers.Append("TraceID", TraceContext.TraceID);

            using (LogContext.PushProperty("TraceID", TraceContext.TraceID))
            {
                await _next(context);
            }
        }
    }
}
