using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Notifications.Api.Services;
using Microsoft.Extensions.Options;

namespace LoveNet.Notifications.Api.Consumers;

/// <summary>Seeds the contact read model when Identity registers a user.</summary>
public class UserRegisteredConsumer : KafkaConsumerService<UserRegisteredEvent>
{
    private const string ConsumerName = "notifications-service";

    public UserRegisteredConsumer(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger<UserRegisteredConsumer> logger)
        : base(scopeFactory, producer, options, logger, KafkaTopics.UserRegistered, ConsumerName)
    {
    }

    protected override async Task HandleAsync(IServiceProvider services, EventEnvelope<UserRegisteredEvent> envelope, CancellationToken cancellationToken)
    {
        var dispatch = services.GetRequiredService<IEmailDispatchService>();
        var data = envelope.Data;
        await dispatch.UpsertContactAsync(envelope.MessageId, data.UserId, data.Email, data.UserName, cancellationToken);
    }
}
