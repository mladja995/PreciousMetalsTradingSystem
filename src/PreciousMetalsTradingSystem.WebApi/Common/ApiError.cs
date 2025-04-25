namespace PreciousMetalsTradingSystem.WebApi.Common
{
    public class ApiError(string code, string message)
    {
        public string Code { get; } = code;
        public string Message { get; } = message;
    }
}
