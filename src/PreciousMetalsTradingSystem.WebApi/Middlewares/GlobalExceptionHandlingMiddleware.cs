using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;
using PreciousMetalsTradingSystem.WebApi.Common;
using PreciousMetalsTradingSystem.WebApi.Common.Exceptions;
using System.Security.Claims;
using System.Text.Json;

namespace PreciousMetalsTradingSystem.WebApi.Middlewares
{
    public class GlobalExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
            _exceptionHandlers = [];

            // Register known exception types and handlers.
            RegisterExceptionHandler<ValidationException>(HandleValidationException);
            RegisterExceptionHandler<NotFoundException>(HandleNotFoundException);
            RegisterExceptionHandler<ConflictException>(HandleConflictException);
            //RegisterExceptionHandler<DomainRuleViolationException>(HandleDomainRuleViolationException);
            RegisterExceptionHandler<ConfigurationException>(HandleConfigurationException);
            RegisterExceptionHandler<ModelBindingException>(HandleModelBindingException);
            RegisterExceptionHandler<UnauthorizedAccessException>(HandleUnauthorizedAccessException);
            RegisterExceptionHandler<ForbiddenAccessException>(HandleForbiddenAccessException); 
            RegisterExceptionHandler<PropertyAccessException>(HandlePropertyAccessException);
        }

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(httpContext, exception);
            }
        }

        private void RegisterExceptionHandler<TException>(Func<HttpContext, TException, Task> handler)
            where TException : Exception
        {
            _exceptionHandlers[typeof(TException)] = async (context, ex) => await handler(context, (TException)ex);
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            foreach (var entry in _exceptionHandlers)
            {
                if (entry.Key.IsInstanceOfType(exception))
                {
                    await entry.Value(httpContext, exception);
                    return;
                }
            }

            // Handle unknown exceptions (500 Internal Server Error).
            await HandleUnknownException(httpContext, exception);
        }

        private async Task HandleUnknownException(HttpContext httpContext, Exception exception)
        {
            _logger.LogError(exception, $"Exception occurred: {exception.Message}", exception.Message);
            httpContext.Items.TryGetValue("TraceID", out var traceId);

            var errorDetails = _env.IsProduction() 
                ? new ApiError("INTERNAL_SERVER_ERROR", $"An unexpected error occurred." + (traceId is not null ? $"Please contact support with TraceId: {traceId}" : string.Empty)) 
                : new ApiError("INTERNAL_SERVER_ERROR",  $"TraceID: {traceId}, {exception.Message} {exception.StackTrace}");
            
            var apiResponse = ApiResponse.Failure(
                errorDetails,
                StatusCodes.Status500InternalServerError
            );

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = JsonSerializer.Serialize(apiResponse);
            await httpContext.Response.WriteAsync(response);
        }

        private async Task HandleValidationException(HttpContext httpContext, Exception exception)
        {
            var validationException = (ValidationException)exception;

            var errors = validationException.Errors
                .SelectMany(e => e.Value.Select(msg => new ApiError(validationException.Code, msg)));

            var apiResponse = ApiResponse.Failure(errors, StatusCodes.Status400BadRequest);

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = JsonSerializer.Serialize(apiResponse);
            await httpContext.Response.WriteAsync(response);
        }

        private async Task HandleNotFoundException(HttpContext httpContext, Exception exception)
        {
            var notFoundException = (NotFoundException)exception;

            var apiResponse = ApiResponse.Failure(
                new ApiError(notFoundException.Code, notFoundException.Message),
                StatusCodes.Status404NotFound
            );

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

            var response = JsonSerializer.Serialize(apiResponse);
            await httpContext.Response.WriteAsync(response);
        }

        private async Task HandleConflictException(HttpContext httpContext, Exception exception)
        {
            var conflictException = (ConflictException)exception;

            var apiResponse = ApiResponse.Failure(
                new ApiError(conflictException.Code, conflictException.Message),
                StatusCodes.Status409Conflict
            );

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            var response = JsonSerializer.Serialize(apiResponse);
            await httpContext.Response.WriteAsync(response);
        }

        private async Task HandleDomainRuleViolationException(HttpContext httpContext, Exception exception)
        {
            var domainException = (DomainRuleViolationException)exception;

            var apiResponse = ApiResponse.Failure(
                new ApiError(domainException.Code, domainException.Message),
                StatusCodes.Status400BadRequest
            );

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = JsonSerializer.Serialize(apiResponse);
            await httpContext.Response.WriteAsync(response);
        }

        private async Task HandleConfigurationException(HttpContext httpContext, Exception exception)
        {
            var configException = (ConfigurationException)exception;

            var apiResponse = ApiResponse.Failure(
                new ApiError(configException.Code, configException.Message),
                StatusCodes.Status500InternalServerError
            );

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = JsonSerializer.Serialize(apiResponse);
            await httpContext.Response.WriteAsync(response);
        }

        private async Task HandleModelBindingException(HttpContext httpContext, Exception exception)
        {
            var modelBindingException = (ModelBindingException)exception;

            var apiResponse = ApiResponse.Failure(
                new ApiError(modelBindingException.Code, modelBindingException.Message),
                StatusCodes.Status400BadRequest
            );

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = JsonSerializer.Serialize(apiResponse);
            await httpContext.Response.WriteAsync(response);
        }

        private async Task HandleUnauthorizedAccessException(HttpContext httpContext, Exception exception)
        {
            var unauthorizedAccessException = (UnauthorizedAccessException)exception;

            var apiResponse = ApiResponse.Failure(
                new ApiError("UNAUTHORIZED_ACCESS", unauthorizedAccessException.Message),
                StatusCodes.Status401Unauthorized
            );

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;


            var response = JsonSerializer.Serialize(apiResponse);
            await httpContext.Response.WriteAsync(response);
        }

        private async Task HandlePropertyAccessException(HttpContext httpContext, Exception exception)
        {
            var propertyAccessException = (PropertyAccessException)exception;

            var apiResponse = ApiResponse.Failure(
                new ApiError(propertyAccessException.Code, propertyAccessException.Message),
                StatusCodes.Status400BadRequest
            );

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = JsonSerializer.Serialize(apiResponse);
            await httpContext.Response.WriteAsync(response);
        }

        private async Task HandleForbiddenAccessException(HttpContext httpContext, Exception exception)
        {
            var forbiddenAccessException = (ForbiddenAccessException)exception;

            var apiResponse = ApiResponse.Failure(
                new ApiError(forbiddenAccessException.Code, forbiddenAccessException.Message),
                StatusCodes.Status403Forbidden
            );

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

            var response = JsonSerializer.Serialize(apiResponse);
            await httpContext.Response.WriteAsync(response);
        }
    }
}
