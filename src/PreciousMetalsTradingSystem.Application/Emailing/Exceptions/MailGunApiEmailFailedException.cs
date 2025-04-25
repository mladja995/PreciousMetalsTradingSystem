using System.Net;

namespace PreciousMetalsTradingSystem.Application.Emailing.Exceptions
{
    public class MailGunApiEmailFailedException : Exception
    {
        public Guid MessageId { get; }
        public HttpStatusCode StatusCode { get; }
        public string ErrorMessage { get; }

        public MailGunApiEmailFailedException(Guid messageId, HttpStatusCode statusCode, string errorMessage) :
            base($"MailGun send e-mail failed. MessageId: {messageId}. StatusCode: {statusCode}. Error: {errorMessage}")
        {
            MessageId = messageId;
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
        }
    }
}
