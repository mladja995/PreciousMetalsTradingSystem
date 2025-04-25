using System.Collections.Concurrent;

namespace PreciousMetalsTradingSystem.Application.Common.Locking
{
    /// <summary>
    /// A thread-safe in-memory implementation of <see cref="ILockManager"/> for managing locks.
    /// Uses <see cref="SemaphoreSlim"/> to provide synchronization for resources identified by unique keys.
    /// </summary>
    public class LockManager : ILockManager
    {
        // A thread-safe dictionary to store semaphores for each key.
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        /// <inheritdoc />
        public async Task<bool> AcquireLockAsync(string key, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            // Get or create a semaphore for the specified key.
            var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            // Attempt to acquire the lock within the specified timeout.
            return await semaphore.WaitAsync(timeout, cancellationToken);
        }

        /// <inheritdoc />
        public void ReleaseLock(string key)
        {
            // Retrieve the semaphore for the specified key.
            if (!_locks.TryGetValue(key, out var semaphore))
            {
                throw new InvalidOperationException($"No lock exists for key: {key}");
            }

            // Release the semaphore.
            semaphore.Release();
        }
    }
}
