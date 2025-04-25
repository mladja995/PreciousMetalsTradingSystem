using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;

namespace PreciousMetalsTradingSystem.WebApi.Common.Exceptions
{
    /// <summary>
    /// Exception for forbidden access scenarios in the application.
    /// Inherits from TradingSystemApplicationException and provides a specific message for access denial.
    /// </summary>
    public class ForbiddenAccessException : TradingSystemApplicationException
    {
        private const string CODE = "FORBIDDEN_ACCESS";
        private const string MESSAGE = "You do not have permission to perform this action. Contact your administrator if you believe this is an error.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class with the default message and code.
        /// </summary>
        public ForbiddenAccessException()
            : base(MESSAGE, CODE)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class with a custom message.
        /// </summary>
        /// <param name="customMessage">A custom error message describing the forbidden access.</param>
        public ForbiddenAccessException(string customMessage)
            : base(customMessage, CODE)
        {
        }
    }
}
