using PreciousMetalsTradingSystem.WebApi.Common.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PreciousMetalsTradingSystem.WebApi.Filters
{
    /// <summary>
    /// This filter replaces the default model state validation behavior (after suppressing `ModelStateInvalidFilter`)
    /// and throws a custom exception when the model state is invalid, without returning the detailed model state errors.
    /// </summary>
    public class ModelBindingValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                throw new ModelBindingException();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No additional action needed after action execution
        }
    }
}
