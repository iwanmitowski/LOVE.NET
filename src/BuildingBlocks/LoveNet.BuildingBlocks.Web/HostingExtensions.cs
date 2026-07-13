using LoveNet.BuildingBlocks.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

namespace LoveNet.BuildingBlocks.Web;

public static class HostingExtensions
{
    /// <summary>
    /// Cross-cutting defaults every LoveNet service shares: structured Serilog
    /// logging, OpenTelemetry traces/metrics (OTLP export when
    /// OTEL_EXPORTER_OTLP_ENDPOINT is set), and ProblemDetails error responses.
    /// </summary>
    public static WebApplicationBuilder AddLoveNetDefaults(this WebApplicationBuilder builder, string serviceName)
    {
        // Entities ported from the monolith mix DateTime kinds (seed data,
        // birthdates); legacy behavior maps them to "timestamp without time
        // zone" like the monolith did. Harmless for services without Npgsql.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        builder.Host.UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", serviceName);

            if (context.HostingEnvironment.IsDevelopment())
            {
                loggerConfiguration.WriteTo.Console();
            }
            else
            {
                loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter());
            }
        });

        var openTelemetry = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddNpgsql())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());

        if (!string.IsNullOrEmpty(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            openTelemetry
                .WithTracing(tracing => tracing.AddOtlpExporter())
                .WithMetrics(metrics => metrics.AddOtlpExporter());
        }

        builder.Services.AddProblemDetails();

        return builder;
    }

    /// <summary>Standard early pipeline: exception handling → request logging → correlation id.</summary>
    public static WebApplication UseLoveNetDefaults(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseSerilogRequestLogging();
        app.UseMiddleware<CorrelationIdMiddleware>();

        return app;
    }

    /// <summary>
    /// Single CORS policy for the SPA origin (config key "Frontend:Origin").
    /// Credentials are required for the refresh-token cookie and SignalR.
    /// </summary>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origin = configuration["Frontend:Origin"] ?? "http://localhost:3000";

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy => policy
                .WithOrigins(origin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });

        return services;
    }
}
