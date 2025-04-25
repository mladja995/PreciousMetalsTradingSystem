using PreciousMetalsTradingSystem.Application.Emailing.Options;
using Microsoft.Extensions.Options;

namespace PreciousMetalsTradingSystem.WebApi.AppConfigSetup
{
    public class MailGunOptionsSetup(IConfiguration configuration) : OptionsSetup<MailGunOptions>(configuration, SectionName)
    {
        private const string SectionName = "MailGun";
    }
}
