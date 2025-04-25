using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;

namespace PreciousMetalsTradingSystem.Application.Common.Exceptions
{
    /// <summary>
    /// Exception raised when there is a conflict with the current state of the resource.
    /// Typically thrown for situations like duplicate data or versioning conflicts.
    /// </summary>
    public class ConflictException : TradingSystemApplicationException
    {
        public ConflictException(string message, string code = "CONFLICT_ERROR")
            : base(message, code)
        {
        }
    }
}
