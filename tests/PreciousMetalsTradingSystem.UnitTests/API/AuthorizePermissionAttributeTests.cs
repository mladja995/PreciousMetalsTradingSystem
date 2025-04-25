using System.Security.Claims;
using PreciousMetalsTradingSystem.WebApi.Common.Authorization;
using PreciousMetalsTradingSystem.WebApi.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using PreciousMetalsTradingSystem.WebApi.Common;

namespace PreciousMetalsTradingSystem.UnitTests.API
{
    public class AuthorizePermissionAttributeTests
    {
        [Fact]
        public async Task OnAuthorizationAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenitcated()
        {
            // Arrange
            var attribute = new AuthorizePermissionAttribute(Permission.ManageProducts);
            var context = CreateAuthorizationFilterContext(isAuthenticated: false);

            // Act           
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await attribute.OnAuthorizationAsync(context));

            // Assert
            Assert.Equal("User is not authenticated.", exception.Message);
        }

        [Fact]
        public async Task OnAuthorizationAsync_ShouldReturnForbidden_WhenUserDoesNotHavePermissions()
        {
            // Arrange
            var attribute = new AuthorizePermissionAttribute(Permission.Trading);
            var context = CreateAuthorizationFilterContext(isAuthenticated: true, userPermissions: new[] { "ManagePositions" });

            // Act
            var exception = await Assert.ThrowsAsync<ForbiddenAccessException>(async () => await attribute.OnAuthorizationAsync(context));

            // Assert
            Assert.Equal("You do not have permission to perform this action. Contact your administrator if you believe this is an error.", exception.Message);
        }

        [Fact]
        public async Task OnAuthorizationAsync_ShouldSucceed_ForUserWithRequiredPermission()
        {
            // Arrange
            var attribute = new AuthorizePermissionAttribute(Permission.ManageProducts); 
            var context = CreateAuthorizationFilterContext(isAuthenticated: true, userPermissions: new[] { "ManageProducts" });

            // Act
            await attribute.OnAuthorizationAsync(context);

            // Assert
            Assert.Null(context.Result); 
        }

        private static AuthorizationFilterContext CreateAuthorizationFilterContext(bool isAuthenticated, string[]? userPermissions = null)
        {
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>();

            if (isAuthenticated)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, "Test User"));
                if (userPermissions != null)
                {
                    claims.AddRange(userPermissions.Select(p => new Claim(Constants.PermissionsClaimType, p)));
                }
            }

            var identity = new ClaimsIdentity(claims, isAuthenticated ? "Test Auth Type" : null);
            httpContext.User = new ClaimsPrincipal(identity);

            var actionContext = new ActionContext(httpContext, 
                                                  new RouteData(), 
                                                  new ActionDescriptor());

            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }
    }
}
