using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Chat.Api.Data;
using LoveNet.Chat.Api.Services;
using Microsoft.Extensions.Options;

namespace LoveNet.Chat.Api.Consumers;

/// <summary>
/// Persists messages published by the Realtime hub. Idempotent by message id
/// (the hub assigns it before producing), so Kafka redelivery is harmless.
/// </summary>
public class MessageSentConsumer : KafkaConsumerService<MessageSentEvent>
{
    private const string ConsumerName = "chat-service";

    public MessageSentConsumer(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger<MessageSentConsumer> logger)
        : base(scopeFactory, producer, options, logger, KafkaTopics.MessageSent, ConsumerName)
    {
    }

    protected override async Task HandleAsync(IServiceProvider services, EventEnvelope<MessageSentEvent> envelope, CancellationToken cancellationToken)
    {
        var store = services.GetRequiredService<IMessageStore>();
        var data = envelope.Data;

        await store.AppendAsync(
            new Message
            {
                Id = data.Id,
                RoomId = data.RoomId,
                SenderId = data.SenderId,
                Text = data.Text,
                ImageUrl = data.ImageUrl,
                CreatedOn = data.CreatedOn,
            },
            cancellationToken);
    }
}
