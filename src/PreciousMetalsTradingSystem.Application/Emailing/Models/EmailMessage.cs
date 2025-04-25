using System.Collections.Generic;

namespace PreciousMetalsTradingSystem.Application.Emailing.Models
{
    public class EmailMessage
    {
        public Guid Id { get; private set; }
        public EmailAddress From { get; private set; }

        public List<EmailAddress> To { get; private set; }
        public List<EmailAddress> Cc { get; private set; }
        public List<EmailAddress> Bcc { get; private set; }

        public string Subject { get; private set; }
        public string Body { get; private set; }

        public EmailMessage()
        {
            Id = Guid.NewGuid();
            Bcc = new List<EmailAddress>();
            Cc = new List<EmailAddress>();
            To = new List<EmailAddress>();
        }

        public EmailMessage(string subject, string body, EmailAddress fromAddress, List<EmailAddress> toAddresses)
            : this()
        {
            From = fromAddress;
            To = toAddresses;
            Subject = subject;
            Body = body;
        }

        public EmailMessage(string subject, string body, EmailAddress fromAddress, List<EmailAddress> toAddresses, List<EmailAddress> ccAddresses, List<EmailAddress> bccAddresses)
            : this(subject, body, fromAddress, toAddresses)
        {
            Cc = ccAddresses;
            Bcc = bccAddresses;
        }

        internal void SetFrom(string address, string name)
        {
            From = new EmailAddress(address, name);
        }
    }
}
