using Microsoft.AspNetCore.Mvc.Filters;

namespace PreciousMetalsTradingSystem.WebApi.Filters
{
    /// <summary>
    /// RouteToCommandPropertyMappingFilter is an action filter that automatically maps route parameters
    /// to corresponding properties in the command/query object passed in the request body. 
    /// This filter ensures that if a property in the command/query object shares the same name and type as a route parameter,
    /// the property is automatically set to the value from the route parameter.<br></br>
    /// Usage:<br></br>
    /// - This filter is useful in scenarios where the command/query object is used for handling API requests, 
    ///   and part of the data comes from the route (e.g., IDs) while the rest comes from the request body.<br></br>
    /// - This eliminates the need to manually assign route parameters to the command's properties inside controller actions.<br></br>
    /// Example:<br></br>
    /// - If you have a route parameter {id} and a command object with a property "Id", this filter will automatically
    ///   set the command's "Id" property to the value of the "id" route parameter.
    /// </summary>
    public class RouteToRequestPropertyMappingFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Access action arguments AFTER model binding (parameters are already converted)
            foreach (var parameter in context.ActionArguments)
            {
                var parameterValue = parameter.Value;
                var parameterType = parameterValue?.GetType();

                if (parameterType != null && (parameterType.Name.EndsWith("Command") || parameterType.Name.EndsWith("Query")))
                {
                    foreach (var prop in parameterType.GetProperties())
                    {
                        // Check if the action arguments (bound parameters) contain a matching key and type
                        var matchingArgumentKey = context.ActionArguments.Keys
                            .FirstOrDefault(key => string.Equals(key, prop.Name, StringComparison.OrdinalIgnoreCase));

                        if (matchingArgumentKey != null && context.ActionArguments[matchingArgumentKey] != null &&
                            prop.PropertyType == context.ActionArguments[matchingArgumentKey]!.GetType())
                        {
                            // Set the command's property from the already converted action argument
                            prop.SetValue(parameterValue, context.ActionArguments[matchingArgumentKey]);
                        }
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No additional logic needed after execution
        }
    }
}
