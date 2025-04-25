using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;

namespace PreciousMetalsTradingSystem.Application.Common.Exceptions
{
    /// <summary>
    /// Exception for configuration errors.
    /// Thrown when a configuration is missing or invalid, commonly in Dependency Injection setup.
    /// Can be thrown from any layer except the domain.
    /// </summary>
    public class ConfigurationException : TradingSystemApplicationException
    {
        public ConfigurationException(string message, string code = "CONFIGURATION_ERROR")
            : base(message, code)
        {
        }
    }
}
