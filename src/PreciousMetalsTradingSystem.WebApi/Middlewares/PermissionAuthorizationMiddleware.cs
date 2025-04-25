using PreciousMetalsTradingSystem.WebApi.Common.Authorization.Services;
using PreciousMetalsTradingSystem.WebApi.Common.Authorization;
using System.Security.Claims;
using PreciousMetalsTradingSystem.WebApi.Common;

namespace PreciousMetalsTradingSystem.WebApi.Middlewares
{
    public class PermissionAuthorizationMiddleware : IMiddleware
    {
        private readonly IRolePermissionService _rolePermissionService;

        public PermissionAuthorizationMiddleware(
            IRolePermissionService rolePermissionService)
        {
            _rolePermissionService = rolePermissionService;
        }

        /// <summary>
        /// Middleware that processes the authenticated user's claims to map roles to permissions.
        /// Extracts roles from the user's claims, retrieves associated permissions for each role, 
        /// and adds the permissions as claims to the HttpContext.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="next">The next middleware in the request pipeline.</param>
        /// <remarks>
        /// - If the user is not authenticated, the middleware skips processing and passes control to the next middleware.
        /// - If roles are present in the user's claims, it resolves the corresponding permissions
        ///   using the IRolePermissionService and appends them to the user's claims.
        /// </remarks>
        /// <returns>A task that represents the completion of request processing.</returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Check if the user is authenticated
            if (context.User?.Identity is null || !context.User.Identity.IsAuthenticated)
            {
                await next(context);
            }

            // Extract roles from user claims
            var userRoles = context.User!.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList() ?? [];

            var permissions = userRoles
                .Select(roleString => Enum.TryParse<Role>(roleString, out var role)
                    ? _rolePermissionService.GetPermissionsForRole(role)
                    : [])
                .SelectMany(rolePermissions => rolePermissions) 
                .Distinct() 
                .ToList();

            // Add permissions to HttpContext as claims
            var permissionClaims = permissions
                .Select(permission => new Claim(Constants.PermissionsClaimType, permission.ToString()));

            if (context.User.Identity is ClaimsIdentity identity)
            {
                identity.AddClaims(permissionClaims);
            }

            await next(context);
        }
    }
}
