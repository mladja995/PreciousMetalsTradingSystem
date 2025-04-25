using PreciousMetalsTradingSystem.Infrastructure.Jobs.Options;
using Microsoft.Extensions.Options;

namespace PreciousMetalsTradingSystem.WebApi.AppConfigSetup
{
    public class HangfireOptionsSetup(IConfiguration configuration) : OptionsSetup<HangfireOptions>(configuration, SectionName)
    {
        public const string SectionName = "Hangfire";
    }
}
