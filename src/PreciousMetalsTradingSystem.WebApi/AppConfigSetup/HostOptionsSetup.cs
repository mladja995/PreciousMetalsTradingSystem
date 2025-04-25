using Microsoft.Extensions.Options;

namespace PreciousMetalsTradingSystem.WebApi.AppConfigSetup
{
    public class HostOptionsSetup(IConfiguration configuration) : OptionsSetup<Options.HostOptions>(configuration, SectionName)
    {
        public static string SectionName => "Host";
    }
}
