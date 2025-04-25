using PreciousMetalsTradingSystem.WebApi.Common.Authorization;
using PreciousMetalsTradingSystem.WebApi.Common.Authorization.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.API
{
    public class RolePermissionServiceTests
    {
        private readonly IConfiguration _mockConfiguration;
        private readonly Mock<ILogger<RolePermissionService>> _mockLogger;
        private readonly RolePermissionService _service;

        public RolePermissionServiceTests()
        {
            var settings = new Dictionary<string, string>
            {
                { "Authorization:RolePermissions:TradingSystemTrader:0", "ViewAllData" },
                { "Authorization:RolePermissions:TradingSystemTrader:1", "ManageProducts" },
                { "Authorization:RolePermissions:TradingSystemTrader:2", "ManageTrades" }
            };
            _mockConfiguration = new ConfigurationBuilder()
                                .AddInMemoryCollection(settings!)
                                .Build();
            _mockLogger = new Mock<ILogger<RolePermissionService>>();

            // Initialize the service
            _service = new RolePermissionService(_mockConfiguration, _mockLogger.Object);
        }

        [Fact]
        public void GetPermissionsForRole_ReturnsEmpty_WhenRoleDoesNotHavePermissions()
        {
            // Act
            var permissions = _service.GetPermissionsForRole(Role.TradingSystemReader);

            // Assert
            Assert.Empty(permissions);
        }

        [Fact]
        public void GetPermissionsForRole_ReturnsPermissions_WhenRoleHasPermissions()
        {
            // Act
            var permissions = _service.GetPermissionsForRole(Role.TradingSystemTrader);

            // Assert
            Assert.NotEmpty(permissions);  
            Assert.Contains(Permission.ViewAllData, permissions);
            Assert.Contains(Permission.ManageProducts, permissions);
            Assert.Contains(Permission.ManageTrades, permissions);
        }
    }

}
