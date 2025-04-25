using PreciousMetalsTradingSystem.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace PreciousMetalsTradingSystem.WebApi.AppConfigSetup
{
    public class ApiSettingsOptionsSetup(IConfiguration configuration) : OptionsSetup<ApiSettingsOptions>(configuration, SectionName)
    {
        public static string SectionName => "ApiSettings";
    }
}
