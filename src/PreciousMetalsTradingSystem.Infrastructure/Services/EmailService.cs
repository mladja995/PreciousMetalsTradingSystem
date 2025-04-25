using System.Net;
using PreciousMetalsTradingSystem.Application.Emailing.Exceptions;
using PreciousMetalsTradingSystem.Application.Emailing.Models;
using PreciousMetalsTradingSystem.Application.Emailing.Options;
using PreciousMetalsTradingSystem.Application.Emailing.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using Throw;

namespace PreciousMetalsTradingSystem.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly MailGunOptions _mailGunOptions;

        public EmailService(ILogger<EmailService> logger, IOptions<MailGunOptions> mailGunOptions)
        {
            _logger = logger;
            _mailGunOptions = mailGunOptions.Value;
        }

        public async Task<bool> SendEmailAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Email {emailMessage.Subject} start sending.");

            var client = CreateClient();
            var request = CreateRequest(emailMessage);
            request.Method = Method.Post;

            var response = await client.ExecuteAsync(request);
            response.StatusCode.Throw(() =>
                new MailGunApiEmailFailedException(emailMessage.Id, response.StatusCode, response.Content ?? response.ErrorMessage ?? string.Empty))
                .IfNotEquals(HttpStatusCode.OK);

            _logger.LogInformation($"Email {emailMessage.Subject} successfully sent.");

            return true;
        }

        private RestClient CreateClient()
        {
            RestClientOptions restClientOptions = new RestClientOptions(_mailGunOptions.BaseUrl)
            {
                Authenticator = new HttpBasicAuthenticator("api", _mailGunOptions.APIKey)
            };
            return new RestClient(restClientOptions);
        }

        private RestRequest CreateRequest(EmailMessage emailMessage)
        {
            var request = new RestRequest();

            var to = ParseAdresses(emailMessage.To);
            var cc = ParseAdresses(emailMessage.Cc);
            var bcc = ParseAdresses(emailMessage.Bcc);

            request.AddParameter("domain", _mailGunOptions.DomainName, ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", $"\"{emailMessage.From.Caption}\" <{emailMessage.From.Address}>");
            request.AddParameter("to", to); //Mailgun requires to have to parameter!
            request.AddParameter("cc", cc);
            request.AddParameter("bcc", bcc);
            request.AddParameter("subject", emailMessage.Subject);

            request.AddParameter("html", emailMessage.Body ?? "<span></span>");

            return request;
        }

        private string? ParseAdresses(List<EmailAddress> emails)
        {
            if (emails == null || !emails.Any())
                return null;
            else
                return string.Join(",", emails.Select(item => item.Address).ToArray());
        }
    }
}
