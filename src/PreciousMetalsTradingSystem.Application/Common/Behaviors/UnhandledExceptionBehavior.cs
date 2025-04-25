using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UnhandledExceptionBehavior(ILogger<TRequest> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            if (_httpContextAccessor.HttpContext?.Request is not null)
            {
                throw;
            }

            LogException(ex, request);
            return default!;
        }
    }

    private void LogException(Exception ex, TRequest request)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogError(ex, "Unhandled Exception for Request {RequestName}: {@Request}", requestName, request);
    }
}
