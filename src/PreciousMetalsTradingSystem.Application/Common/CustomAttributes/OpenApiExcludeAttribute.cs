namespace PreciousMetalsTradingSystem.Application.Common.CustomAttributes
{
    /// <summary>
    /// Attribute that excludes properties from Swagger API definition
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OpenApiExcludeAttribute : Attribute
    {
    }
}
