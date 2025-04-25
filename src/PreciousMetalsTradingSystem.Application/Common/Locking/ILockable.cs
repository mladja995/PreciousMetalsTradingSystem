namespace PreciousMetalsTradingSystem.Application.Common.Locking
{
    /// <summary>
    /// Defines a contract for requests that require a single lock key for synchronization.
    /// Implementing this interface allows the request to specify its locking requirement dynamically.
    /// </summary>
    public interface ILockable
    {
        /// <summary>
        /// Retrieves the unique key that should be locked for this request.
        /// </summary>
        /// <returns>A unique lock key required for the request.</returns>
        string GetLockKey();
    }
}
