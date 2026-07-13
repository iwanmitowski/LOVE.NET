using System.Text.Json;
using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;

namespace LoveNet.BuildingBlocks.EventBus;

public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> producer;

    public KafkaEventPublisher(IProducer<string, string> producer)
    {
        this.producer = producer;
    }

    public async Task PublishAsync<T>(string topic, string key, T data, string? correlationId = null, CancellationToken cancellationToken = default)
    {
        var envelope = EventEnvelope<T>.Create(data, correlationId);
        var message = new Message<string, string>
        {
            Key = key,
            Value = JsonSerializer.Serialize(envelope, JsonDefaults.Options),
        };

        await producer.ProduceAsync(topic, message, cancellationToken);
    }
}
