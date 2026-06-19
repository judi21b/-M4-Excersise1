using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Create correlation ID (first 8 chars only)
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        // Add header BEFORE calling next middleware
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        var stopwatch = Stopwatch.StartNew();

        // Log request start
        _logger.LogInformation("START Request {Method} {Path} CorrelationId={Id}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        await _next(context);

        stopwatch.Stop();

        // Log request end
        _logger.LogInformation("END Request Status={StatusCode} ElapsedMs={Elapsed} CorrelationId={Id}",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}