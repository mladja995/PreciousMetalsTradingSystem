namespace PreciousMetalsTradingSystem.WebApi.Middlewares
{
    public class HttpClientLoggingMiddleware : DelegatingHandler
    {
        private readonly ILogger<HttpClientLoggingMiddleware> _logger;

        public HttpClientLoggingMiddleware(ILogger<HttpClientLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Log request
            string requestBody = string.Empty;
            if (request.Content != null)
            {
                requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            _logger.LogInformation("Request: {Method} {RequestUri}\n Headers: {Headers}\n Body: {Body}\n",
                request.Method,
                request.RequestUri,
                request.Headers,
                requestBody);

            // Execute request
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // Log response
            string responseBody = string.Empty;
            if (response.Content != null)
            {
                responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            }

            _logger.LogInformation("Response: {StatusCode}\n Headers: {Headers}\n Body: {Body}\n",
                response.StatusCode,
                response.Headers,
                responseBody);

            return response;
        }
    }
}
