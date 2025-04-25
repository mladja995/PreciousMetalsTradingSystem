using FluentValidation.Results;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.WebApi.Common.Exceptions;
using PreciousMetalsTradingSystem.WebApi.Middlewares;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace PreciousMetalsTradingSystem.UnitTests.API
{
    public class GlobalExceptionHandlingMiddlewareTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandlingMiddleware>> _loggerMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly DefaultHttpContext _httpContext;
        private readonly RequestDelegate _next;
        private readonly GlobalExceptionHandlingMiddleware _middleware;

        public GlobalExceptionHandlingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
            _envMock = new Mock<IWebHostEnvironment>();
            _httpContext = new DefaultHttpContext();
            _next = Mock.Of<RequestDelegate>();
            _middleware = new GlobalExceptionHandlingMiddleware(_loggerMock.Object, _envMock.Object);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleInternalServerError_UnknownException()
        {
            _httpContext.Response.Body = new MemoryStream();
            var middleware = new GlobalExceptionHandlingMiddleware(_loggerMock.Object, _envMock.Object);

            await middleware.InvokeAsync(_httpContext, _ => throw new Exception("Something went wrong"));

            Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            Assert.Contains("INTERNAL_SERVER_ERROR", response);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleBadRequest_ValidationException()
        {
            _httpContext.Response.Body = new MemoryStream();
            var middleware = new GlobalExceptionHandlingMiddleware(_loggerMock.Object, _envMock.Object);

            await middleware.InvokeAsync(_httpContext, _ => throw new ValidationException(
                [new ValidationFailure("One or more validation failures have occurred.", "VALIDATION_ERROR")]));

            Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            Assert.Contains("VALIDATION_ERROR", response);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleNotFound_NotFoundException()
        {
            _httpContext.Response.Body = new MemoryStream();
            var middleware = new GlobalExceptionHandlingMiddleware(_loggerMock.Object, _envMock.Object);

            await middleware.InvokeAsync(_httpContext, _ => throw new NotFoundException("NOT_FOUND", "Resource not found"));

            Assert.Equal((int)HttpStatusCode.NotFound, _httpContext.Response.StatusCode);
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            Assert.Contains("NOT_FOUND", response);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleConflict_ConflictException()
        {
            _httpContext.Response.Body = new MemoryStream();
            var middleware = new GlobalExceptionHandlingMiddleware(_loggerMock.Object, _envMock.Object);

            await middleware.InvokeAsync(_httpContext, _ => throw new ConflictException("CONFLICT_ERROR", "Resource conflict detected"));

            Assert.Equal((int)HttpStatusCode.Conflict, _httpContext.Response.StatusCode);
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            Assert.Contains("CONFLICT_ERROR", response);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleInternalServerError_ConfigurationException()
        {
            _httpContext.Response.Body = new MemoryStream();
            var middleware = new GlobalExceptionHandlingMiddleware(_loggerMock.Object, _envMock.Object);

            await middleware.InvokeAsync(_httpContext, _ => throw new ConfigurationException("Something went wrong"));

            Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            Assert.Contains("CONFIGURATION_ERROR", response);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleBadRequest_ModelBindingException()
        {
            _httpContext.Response.Body = new MemoryStream();
            var middleware = new GlobalExceptionHandlingMiddleware(_loggerMock.Object, _envMock.Object);

            await middleware.InvokeAsync(_httpContext, _ => throw new ModelBindingException());

            Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            Assert.Contains("MODEL_BINDING_ERROR", response);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleUnauthorized_UnauthorizedAccessException()
        {
            _httpContext.Response.Body = new MemoryStream();

            var middleware = new GlobalExceptionHandlingMiddleware(_loggerMock.Object, _envMock.Object);

            await middleware.InvokeAsync(_httpContext, _ => throw new UnauthorizedAccessException("Unauthorized access"));

            Assert.Equal((int)HttpStatusCode.Unauthorized, _httpContext.Response.StatusCode);
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            Assert.Contains("UNAUTHORIZED_ACCESS", response);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleInternalServerError_PropertyAccessException()
        {
            _httpContext.Response.Body = new MemoryStream();
            var middleware = new GlobalExceptionHandlingMiddleware(_loggerMock.Object, _envMock.Object);

            await middleware.InvokeAsync(_httpContext, _ => throw new PropertyAccessException("Something went wrong"));

            Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            Assert.Contains("PROPERTY_ACCESS_ERROR", response);
        }

        [Fact]
        public async Task InvokeAsync_ShouldHandleForbidden_ForbiddenAccessException()
        {
            _httpContext.Response.Body = new MemoryStream();
            var middleware = new GlobalExceptionHandlingMiddleware(_loggerMock.Object, _envMock.Object);

            await middleware.InvokeAsync(_httpContext, _ => throw new ForbiddenAccessException(
                "You do not have permission to perform this action. Contact your administrator if you believe this is an error."));

            Assert.Equal((int)HttpStatusCode.Forbidden, _httpContext.Response.StatusCode);
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            Assert.Contains("FORBIDDEN_ACCESS", response);
        }
    }
}
