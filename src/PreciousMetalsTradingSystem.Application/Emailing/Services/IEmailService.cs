using PreciousMetalsTradingSystem.Application.Emailing.Models;

namespace PreciousMetalsTradingSystem.Application.Emailing.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(
            EmailMessage emailMessage,
            CancellationToken cancellationToken = default);
    }
}
