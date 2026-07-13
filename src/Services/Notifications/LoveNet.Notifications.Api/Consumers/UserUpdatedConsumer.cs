using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Notifications.Api.Services;
using Microsoft.Extensions.Options;

namespace LoveNet.Notifications.Api.Consumers;

/// <summary>Keeps the contact read model fresh when Profile republishes a user.</summary>
public class UserUpdatedConsumer : KafkaConsumerService<UserUpdatedEvent>
{
    private const string ConsumerName = "notifications-service";

    public UserUpdatedConsumer(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger<UserUpdatedConsumer> logger)
        : base(scopeFactory, producer, options, logger, KafkaTopics.UserUpdated, ConsumerName)
    {
    }

    protected override async Task HandleAsync(IServiceProvider services, EventEnvelope<UserUpdatedEvent> envelope, CancellationToken cancellationToken)
    {
        var dispatch = services.GetRequiredService<IEmailDispatchService>();
        var data = envelope.Data;
        await dispatch.UpsertContactAsync(envelope.MessageId, data.UserId, data.Email, data.UserName, cancellationToken);
    }
}
