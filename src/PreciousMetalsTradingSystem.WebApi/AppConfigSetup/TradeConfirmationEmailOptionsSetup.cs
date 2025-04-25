using PreciousMetalsTradingSystem.Application.Emailing.Options;
using Microsoft.Extensions.Options;

namespace PreciousMetalsTradingSystem.WebApi.AppConfigSetup
{
    public class TradeConfirmationEmailOptionsSetup(IConfiguration configuration) : OptionsSetup<TradeConfirmationEmailOptions>(configuration, SectionName)
    {
        private const string SectionName = "TradeConfirmationEmail";
    }
}
