namespace PreciousMetalsTradingSystem.Application.Common.CustomAttributes
{
    /// <summary>
    /// Specifies that a property is optional and should not be validated 
    /// for existence or non-null values in the configuration validation process.
    /// This attribute can be applied only to properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionalAttribute : Attribute
    {
    }
}
