using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;


namespace PreciousMetalsTradingSystem.IntegrationTests.Factories
{
    public class TradingSystemApiWebApplicationFactory<TStartup> 
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development"); // Ensure the environment is set to Development

            builder.ConfigureAppConfiguration(config =>
            {
                config.AddConfiguration(new ConfigurationBuilder()
                    .AddJsonFile("appsettings.Test.json")
                    .Build());
            });

            // Add the "--integrationTestsRunning=true" argument to simulate test mode
            builder.UseSetting(WebHostDefaults.ApplicationKey, typeof(Program).Assembly.FullName)
                   .UseSetting("integrationTestsRunning", "true");
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            try
            {
                return base.CreateHost(builder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateHost: {ex}");
                throw;
            }
        }
    }
}
