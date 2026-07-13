using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Notifications.Api.Services;
using Microsoft.Extensions.Options;

namespace LoveNet.Notifications.Api.Consumers;

/// <summary>Sends the transactional emails Identity requests (verify, reset password).</summary>
public class EmailRequestedConsumer : KafkaConsumerService<EmailRequestedEvent>
{
    private const string ConsumerName = "notifications-service";

    public EmailRequestedConsumer(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger<EmailRequestedConsumer> logger)
        : base(scopeFactory, producer, options, logger, KafkaTopics.EmailRequested, ConsumerName)
    {
    }

    protected override async Task HandleAsync(IServiceProvider services, EventEnvelope<EmailRequestedEvent> envelope, CancellationToken cancellationToken)
    {
        var dispatch = services.GetRequiredService<IEmailDispatchService>();
        await dispatch.HandleEmailRequestedAsync(envelope.MessageId, envelope.Data, cancellationToken);
    }
}
