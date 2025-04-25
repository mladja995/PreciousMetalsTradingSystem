namespace PreciousMetalsTradingSystem.Application.Common.Locking
{
    /// <summary>
    /// Defines a contract for managing locks based on unique keys.
    /// Provides methods to acquire and release locks for ensuring thread-safe operations.
    /// </summary>
    public interface ILockManager
    {
        /// <summary>
        /// Attempts to acquire a lock for the specified key within the given timeout period.
        /// </summary>
        /// <param name="key">The unique key representing the resource to lock.</param>
        /// <param name="timeout">The maximum time to wait for acquiring the lock.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that resolves to <c>true</c> if the lock was successfully acquired within the timeout period,
        /// or <c>false</c> otherwise.
        /// </returns>
        Task<bool> AcquireLockAsync(string key, TimeSpan timeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Releases a lock for the specified key.
        /// </summary>
        /// <param name="key">The unique key representing the resource to unlock.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if there is no lock associated with the specified key.
        /// </exception>
        void ReleaseLock(string key);
    }

}
