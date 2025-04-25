namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    /// <summary>
    /// Base class for all custom exceptions in the application.
    /// Provides a consistent structure for error codes and messages.
    /// </summary>
    public abstract class TradingSystemApplicationException : Exception
    {
        public string Code { get; }

        protected TradingSystemApplicationException(string message, string code) : base(message)
        {
            Code = code;
        }
    }
}
