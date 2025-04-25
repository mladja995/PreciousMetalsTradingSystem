using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;

namespace PreciousMetalsTradingSystem.Application.Common.Exceptions
{
    public class PropertyAccessException : TradingSystemApplicationException
    {

        private const string CODE = "PROPERTY_ACCESS_ERROR";
        public PropertyAccessException(string message, string code = CODE)
        : base(message,code)
        {
        }

        public PropertyAccessException(string propertyName)
        : base($"Failed to access property '{propertyName}'.",CODE)
        {
        }
    }
}
