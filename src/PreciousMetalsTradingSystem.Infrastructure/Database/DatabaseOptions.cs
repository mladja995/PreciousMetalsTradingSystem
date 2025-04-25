namespace PreciousMetalsTradingSystem.Infrastructure.Database
{
    public class DatabaseOptions
    {
        public required string ConnectionString { get; init; }
        public bool EnableDetailedErrors { get; init; } = false;
        public bool EnableSensitiveDataLogging { get; init; } = false;
    }
}
