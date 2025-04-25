namespace PreciousMetalsTradingSystem.Application.Common
{
    public static class TraceContext
    {
        private static readonly AsyncLocal<string> _traceId = new();

        public static string TraceID
        {
            get => _traceId.Value ??= Guid.NewGuid().ToString(); // Assign a default GUID if not set
            set {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("TraceID cannot be null or whitespace.");
                _traceId.Value = value;
            } 
        }
    }
}
