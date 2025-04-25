namespace PreciousMetalsTradingSystem.Application.AMark.Exceptions
{
    public class AMarkApiException : Exception
    {
        public string ErrorCode { get; }
        public string ErrorMessage { get; }

        public AMarkApiException(string errorCode, string errorMessage)
            : base($"A-Mark API Error: {errorCode} - {errorMessage}")
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }

}
