using PreciousMetalsTradingSystem.Infrastructure.Database;
using Microsoft.Extensions.Options;

namespace PreciousMetalsTradingSystem.WebApi.AppConfigSetup
{
    public class DatabaseOptionsSetup : OptionsSetup<DatabaseOptions>
    {
        public const string SectionName = "Database";

        public DatabaseOptionsSetup(IConfiguration configuration): base( configuration, SectionName)
        {
        }
    }
}
