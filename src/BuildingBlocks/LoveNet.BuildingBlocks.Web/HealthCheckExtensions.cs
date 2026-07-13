using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoveNet.BuildingBlocks.Web;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers /health checks for the dependencies a service actually has.
    /// Connection string name: "Default"; Kafka: "Kafka:BootstrapServers";
    /// Redis: "Redis:Connection".
    /// </summary>
    public static IServiceCollection AddLoveNetHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration,
        bool postgres = false,
        bool kafka = false,
        bool redis = false)
    {
        var healthChecks = services.AddHealthChecks();

        if (postgres)
        {
            healthChecks.AddNpgSql(configuration.GetConnectionString("Default")!);
        }

        if (kafka)
        {
            healthChecks.AddKafka(new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
            });
        }

        if (redis)
        {
            healthChecks.AddRedis(configuration["Redis:Connection"]!);
        }

        return services;
    }

    public static WebApplication MapLoveNetHealth(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}
