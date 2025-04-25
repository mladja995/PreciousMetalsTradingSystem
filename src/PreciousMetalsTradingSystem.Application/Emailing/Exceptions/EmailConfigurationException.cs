using System.Net;

namespace PreciousMetalsTradingSystem.Application.Emailing.Exceptions
{
    public class EmailConfigurationException : Exception
    {
        public EmailConfigurationException(string errorMessage) :
            base(errorMessage)
        {
        }
    }
}
