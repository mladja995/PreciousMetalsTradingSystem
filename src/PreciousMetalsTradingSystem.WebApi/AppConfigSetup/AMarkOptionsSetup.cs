using PreciousMetalsTradingSystem.Application.AMark.Options;
using Microsoft.Extensions.Options;

namespace PreciousMetalsTradingSystem.WebApi.AppConfigSetup
{
    public class AMarkOptionsSetup(IConfiguration configuration) : OptionsSetup<AMarkOptions>(configuration, SectionName)
    {
        private const string SectionName = "AMark";
    }
}
