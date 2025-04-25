using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;

namespace PreciousMetalsTradingSystem.WebApi.Common.Exceptions
{
    /// <summary>
    /// Exception for model binding failures in the application.
    /// Inherits from TradingSystemApplicationException and provides a more expressive message.
    /// </summary>
    public class ModelBindingException : TradingSystemApplicationException
    {
        private const string CODE = "MODEL_BINDING_ERROR";
        private const string MESSAGE = "The request format or data is invalid. Please verify the input and ensure it matches the expected structure.";
        public ModelBindingException()
            : base(MESSAGE, CODE)
        {
        }
    }
}
