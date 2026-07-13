using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Notifications.Api.Services;
using Microsoft.Extensions.Options;

namespace LoveNet.Notifications.Api.Consumers;

/// <summary>Emails both users when Matching creates a new match.</summary>
public class MatchCreatedConsumer : KafkaConsumerService<MatchCreatedEvent>
{
    private const string ConsumerName = "notifications-service";

    public MatchCreatedConsumer(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger<MatchCreatedConsumer> logger)
        : base(scopeFactory, producer, options, logger, KafkaTopics.MatchCreated, ConsumerName)
    {
    }

    protected override async Task HandleAsync(IServiceProvider services, EventEnvelope<MatchCreatedEvent> envelope, CancellationToken cancellationToken)
    {
        var dispatch = services.GetRequiredService<IEmailDispatchService>();
        await dispatch.HandleMatchCreatedAsync(envelope.MessageId, envelope.Data, cancellationToken);
    }
}
