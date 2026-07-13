using System.Threading.RateLimiting;
using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddLoveNetDefaults("gateway");

builder.Services.AddLoveNetJwt(builder.Configuration);
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

// Named clients for the admin statistics aggregation. Standard resilience =
// request timeout + retry with backoff + circuit breaker per service.
foreach (var service in new[] { "Identity", "Profile", "Matching", "Chat" })
{
    var baseAddress = builder.Configuration[$"Services:{service}"]
        ?? throw new InvalidOperationException($"Missing 'Services:{service}' configuration value");

    builder.Services
        .AddHttpClient(service.ToLowerInvariant(), client => client.BaseAddress = new Uri(baseAddress))
        .AddStandardResilienceHandler();
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Coarse per-IP ceiling over everything the gateway serves or proxies.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 0,
            }));

    // Tight limit for credential endpoints (login/register brute-force).
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(60),
                QueueLimit = 0,
            }));

    // Swiping burst control: 5-token bucket refilled at 5 tokens/second.
    options.AddPolicy("likes", context =>
        RateLimitPartition.GetTokenBucketLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 5,
                TokensPerPeriod = 5,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                QueueLimit = 0,
                AutoReplenishment = true,
            }));

    options.OnRejected = (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        return ValueTask.CompletedTask;
    };
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseLoveNetDefaults();

// Downstream services set no CORS headers; the gateway answers for the SPA
// origin on both local endpoints and proxied responses (credentials included
// for the refresh-token cookie and SignalR).
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy();
app.MapHealthChecks("/health");

await app.RunAsync();
