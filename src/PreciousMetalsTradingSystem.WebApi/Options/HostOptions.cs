namespace PreciousMetalsTradingSystem.WebApi.Options
{
    public class HostOptions
    {
        public bool UseOpenApi { get; init; }
        public string? AllowedOrigins { get; init; }
        public bool UseMockAuthentication { get; init; }
    }
}
