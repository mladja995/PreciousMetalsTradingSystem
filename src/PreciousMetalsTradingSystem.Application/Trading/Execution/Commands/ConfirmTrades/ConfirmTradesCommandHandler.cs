using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Emailing.Exceptions;
using PreciousMetalsTradingSystem.Application.Emailing.Models;
using PreciousMetalsTradingSystem.Application.Emailing.Options;
using PreciousMetalsTradingSystem.Application.Emailing.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.Extensions.Options;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.ConfirmTrades
{
    public class ConfirmTradesCommandHandler : IRequestHandler<ConfirmTradesCommand>
    {
        private readonly TradeConfirmationEmailOptions _tradeConfirmationEmailOptions;
        private readonly IRepository<Trade, TradeId> _tradeRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmTradesCommandHandler(
            IOptions<TradeConfirmationEmailOptions> tradeConfirmationEmailOptions,
            IRepository<Trade, TradeId> tradeRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork)
        {
            _tradeConfirmationEmailOptions = tradeConfirmationEmailOptions.Value;
            _tradeRepository = tradeRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ConfirmTradesCommand request, CancellationToken cancellationToken)
        {
            if (_tradeConfirmationEmailOptions.SendTradeConfirmationEmail)
            {
                //STEP: validate email settings
                _tradeConfirmationEmailOptions.FromAddress.Throw(() =>
                    new EmailConfigurationException("Sender Email for Trade Confirmation Emails is not set.")).IfEmpty();
                _tradeConfirmationEmailOptions.ToAddresses.Throw(() =>
                    new EmailConfigurationException("Recipient List for Trade Confirmation Emails is not set.")).IfEmpty();
                _tradeConfirmationEmailOptions.EmailSubject.Throw(() =>
                    new EmailConfigurationException("Email Subject for Trade Confirmation Emails is not set.")).IfEmpty();
                _tradeConfirmationEmailOptions.EmailBody.Throw(() =>
                    new EmailConfigurationException("Email Body for Trade Confirmation Emails is not set.")).IfEmpty();
            }

            var (trades, totalCount) = await _tradeRepository.GetAllAsync(
                filter: x => x.ConfirmedOnUtc == null,
                readOnly: false,
                cancellationToken: cancellationToken);

            foreach (var trade in trades)
            {
                if (_tradeConfirmationEmailOptions.SendTradeConfirmationEmail)
                {
                    //STEP: send trade confirmation email
                    var fromAddress = new EmailAddress(_tradeConfirmationEmailOptions.FromAddress, _tradeConfirmationEmailOptions.FromName);
                    var emailRecipientList = new List<EmailAddress>();
                    foreach (var toAddress in _tradeConfirmationEmailOptions.ToAddresses.Split(","))
                    {
                        var to = new EmailAddress(toAddress, _tradeConfirmationEmailOptions.ToName);
                        emailRecipientList.Add(to);
                    };
                    var subject = _tradeConfirmationEmailOptions.EmailSubject.Replace("#", $"#{trade.TradeNumber}");
                    var body = _tradeConfirmationEmailOptions.EmailBody.Replace("#", $"#{trade.TradeNumber}");

                    var email = new EmailMessage(subject, body, fromAddress, emailRecipientList);
                    await _emailService.SendEmailAsync(email, cancellationToken);
                }

                //STEP: mark trade as Confirmed
                trade.MarkAsConfirmed();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
