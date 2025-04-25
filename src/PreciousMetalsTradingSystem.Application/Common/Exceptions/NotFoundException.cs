using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;

namespace PreciousMetalsTradingSystem.Application.Common.Exceptions
{
    /// <summary>
    /// Exception raised when a requested resource is not found.
    /// Typically used in the application layer when querying repositories or services.
    /// </summary>
    public class NotFoundException : TradingSystemApplicationException
    {
        private const string CODE = "NOT_FOUND";

        public NotFoundException(string message, string code = CODE)
            : base(message, code)
        {
        }

        public NotFoundException(string name, object key)
            : base($"Entity {name} with key '{key}' was not found.", 
                  $"{name?.ToUpper()}.{CODE}")
        {
        }
    }
}
