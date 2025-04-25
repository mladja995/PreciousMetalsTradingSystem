namespace PreciousMetalsTradingSystem.WebApi.Common.Authorization.Services
{
    public interface IRolePermissionService
    {
        IEnumerable<Permission> GetPermissionsForRole(Role role);
    }
}
