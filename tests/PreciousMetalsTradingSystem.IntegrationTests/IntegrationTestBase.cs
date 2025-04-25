using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;
using PreciousMetalsTradingSystem.Infrastructure.Database;
using PreciousMetalsTradingSystem.IntegrationTests.Factories;
using PreciousMetalsTradingSystem.WebApi.Common.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace PreciousMetalsTradingSystem.IntegrationTests
{
    public class IntegrationTestBase : IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly TradingSystemApiWebApplicationFactory<Program> _factory;
        protected readonly HttpClient Client;

        public IntegrationTestBase()
        {
            try
            {
                _factory = new TradingSystemApiWebApplicationFactory<Program>();
                _scope = _factory.Services.CreateScope();
                Client = _factory.CreateClient();
                AddAuthorizationHeader(Client);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IntegrationTestBase initialization: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Gets a repository instance for the specified entity and entity ID types.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TEntityId">The entity ID type.</typeparam>
        /// <returns>An instance of <see cref="IRepository{TEntity, TEntityId}"/>.</returns>
        protected IRepository<TEntity, TEntityId> GetRepository<TEntity, TEntityId>()
            where TEntity : AggregateRoot<TEntityId>
            where TEntityId : ValueObject, IEntityId
            => _scope.ServiceProvider.GetRequiredService<IRepository<TEntity, TEntityId>>();

        /// <summary>
        /// Gets a service instance of the specified type from the DI container.
        /// </summary>
        /// <typeparam name="TService">The type of the service to retrieve.</typeparam>
        /// <returns>An instance of the requested service.</returns>
        protected TService GetService<TService>()
            where TService : class
            => _scope.ServiceProvider.GetRequiredService<TService>();

        public void Dispose()
        {
            _scope.ServiceProvider.GetRequiredService<TradingSystemDbContext>().Database.EnsureDeleted();
            _scope.Dispose();
            Client.Dispose();
            _factory.Dispose();
        }


        #region Private

        private static void AddAuthorizationHeader(HttpClient client)
        {
            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("FakeBearer", token);
        }

        private static string GenerateJwtToken()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "test-user"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, Role.TradingSystemAdministrator.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("f21b2a48-52d7-461f-8342-2ce2f2b5c8a8")); // Use a proper secret key for tests
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "test-issuer",
                audience: "test-audience",
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion Private
    }
}
