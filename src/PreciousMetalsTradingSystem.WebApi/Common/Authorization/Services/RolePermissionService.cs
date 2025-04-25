namespace PreciousMetalsTradingSystem.WebApi.Common.Authorization.Services
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RolePermissionService> _logger;

        public RolePermissionService(
            IConfiguration configuration, 
            ILogger<RolePermissionService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the permissions assigned to a specific role from the configuration.
        /// </summary>
        /// <param name="role">The role for which permissions are retrieved.</param>
        /// <returns>An enumerable of permissions assigned to the role.</returns>
        public IEnumerable<Permission> GetPermissionsForRole(Role role)
        {
            // If role is TradingSystemAdministrator, return all permissions from the Permission enum
            if (role == Role.TradingSystemAdministrator)
            {
                _logger.LogInformation($"Role '{role}' detected. Granting all permissions.");
                return Enum.GetValues(typeof(Permission)).Cast<Permission>();
            }

            // Access the RolePermissions section from the configuration
            var rolePermissionsSection = _configuration.GetSection("Authorization:RolePermissions");

            // Check if the section exists
            if (!rolePermissionsSection.Exists())
            {
                // Log a warning and return an empty enumerable as fallback
                _logger.LogWarning("The RolePermissions section is missing in the configuration.");
                return [];
            }

            // Get the permissions as a list of strings for the specified role
            var permissions = rolePermissionsSection.GetSection(role.ToString()).Get<List<string>>();

            // Check if permissions are defined for the role
            if (permissions == null || permissions.Count == 0)
            {
                _logger.LogWarning("No permissions defined for the role '{Role}' in the configuration.", role);
                return [];
            }

            // Convert the string list to the Permission enum
            return permissions
                .Select(permission => Enum.TryParse(permission, out Permission parsedPermission) ? parsedPermission : (Permission?)null)
                .Where(parsedPermission => parsedPermission.HasValue)
                .Select(parsedPermission => parsedPermission!.Value);
        }
    }
}
