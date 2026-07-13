using Confluent.Kafka;
using LoveNet.BuildingBlocks.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LoveNet.BuildingBlocks.EventBus;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the shared idempotent Kafka producer and <see cref="IEventPublisher"/>.
    /// </summary>
    public static IServiceCollection AddKafkaEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var config = new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers,
                EnableIdempotence = true,
                Acks = Acks.All,
            };

            return new ProducerBuilder<string, string>(config).Build();
        });

        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        return services;
    }

    /// <summary>Starts the outbox relay for the service's DbContext.</summary>
    public static IServiceCollection AddOutboxPublisher<TContext>(this IServiceCollection services)
        where TContext : DbContext
        => services.AddHostedService<OutboxPublisher<TContext>>();
}
