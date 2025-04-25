using PreciousMetalsTradingSystem.WebApi.Common.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PreciousMetalsTradingSystem.WebApi.Common.Authorization
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly Permission[] _allowedPermissions;

        public AuthorizePermissionAttribute(params Permission[] allowedPermissions)
        {
            _allowedPermissions = allowedPermissions;
        }

        /// <summary>
        /// Validates if the user has at least one of the allowed permissions.
        /// </summary>
        /// <param name="context">Authorization filter context.</param>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Retrieve user claims
            var user = context.HttpContext.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Extract permissions from claims
            var userPermissions = user.Claims
                .Where(c => c.Type == Constants.PermissionsClaimType)
                .Select(c => c.Value)
                .ToList();

            // Check if the user has at least one of the allowed permissions
            if (!_allowedPermissions.Any(permission => userPermissions.Contains(permission.ToString())))
            {
                throw new ForbiddenAccessException();
            }

            await Task.CompletedTask;
        }
    }
}
