using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace LoveNet.BuildingBlocks.Web.Middleware;

/// <summary>
/// Reads (or mints) the X-Correlation-Id header, echoes it on the response and
/// pushes it into the Serilog log context. The value also rides along in Kafka
/// event envelopes so a request can be traced across services.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var incoming) && !string.IsNullOrWhiteSpace(incoming)
            ? incoming.ToString()
            : Guid.NewGuid().ToString("N");

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}

public static class CorrelationIdHttpContextExtensions
{
    public static string? GetCorrelationId(this HttpContext context) =>
        context.Items[CorrelationIdMiddleware.HeaderName] as string;
}
