using PreciousMetalsTradingSystem.Application.Common.Locking;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace PreciousMetalsTradingSystem.Application.Common.Behaviors
{
    /// <summary>
    /// A MediatR pipeline behavior for applying a locking mechanism based on the request's lock key.
    /// Ensures thread-safe processing of requests requiring synchronization.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class LockingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILockManager _lockManager;
        private readonly ILogger<LockingBehavior<TRequest, TResponse>> _logger;

        public LockingBehavior(
            ILockManager lockManager, 
            ILogger<LockingBehavior<TRequest, TResponse>> logger)
        {
            _lockManager = lockManager;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is not ILockable lockable)
            {
                // Proceed with the request
                return await next();
            }

            var lockKey = lockable.GetLockKey();
            var timeout = TimeSpan.FromSeconds(5);

            var traceId = TraceContext.TraceID;

            var stopwatch = Stopwatch.StartNew();
            if (!await _lockManager.AcquireLockAsync(lockKey, timeout, cancellationToken))
            {
                _logger.LogError(
                    "Failed to acquire lock for key: {LockKey} within timeout: {TimeoutSeconds}s. Request: {RequestName} {TraceID}",
                    lockKey,
                    timeout.TotalSeconds,
                    typeof(TRequest).Name,
                    traceId);

                throw new InvalidOperationException($"Failed to acquire lock for key: {lockKey}");
            }

            try
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    "Lock acquired for key: {LockKey} by request: {RequestName} {TraceID}. Acquisition time: {ElapsedMilliseconds} ms",
                    lockKey,
                    typeof(TRequest).Name,
                    traceId,
                    stopwatch.ElapsedMilliseconds);

                // Proceed with the request
                return await next();
            }
            finally
            {
                // Release the lock
                _lockManager.ReleaseLock(lockKey);
                _logger.LogInformation(
                    "Lock released for key: {LockKey} by request: {RequestName} {TraceID}",
                    lockKey,
                    typeof(TRequest).Name,
                    traceId);
            }
        }
    }
}
