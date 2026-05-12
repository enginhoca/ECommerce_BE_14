using System;
using System.Diagnostics;

namespace ECommerce.API.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid().ToString("N");
        context.Response.Headers["X-Correalation-Id"]=correlationId;
        _logger.LogInformation(
            "[{CorrelationId}] -> {Method} {Path}",
            correlationId,
            context.Request.Method,
            context.Request.Path
        );

        await _next(context);
        sw.Stop();
        _logger.LogInformation(
            "[{CorrelationId}]<- {StatusCode} {Method} {Path} ({ElapsedMs}ms)",
            correlationId,
            context.Response.StatusCode,
            context.Request.Method,
            context.Request.Path,
            sw.ElapsedMilliseconds
        );
    }
}
