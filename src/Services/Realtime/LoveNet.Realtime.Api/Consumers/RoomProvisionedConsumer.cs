using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Realtime.Api.Services;
using Microsoft.Extensions.Options;

namespace LoveNet.Realtime.Api.Consumers;

/// <summary>Registers public rooms for join authorization (SADD is idempotent).</summary>
public class RoomProvisionedConsumer : KafkaConsumerService<RoomProvisionedEvent>
{
    private const string ConsumerName = "realtime-service";

    public RoomProvisionedConsumer(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger<RoomProvisionedConsumer> logger)
        : base(scopeFactory, producer, options, logger, KafkaTopics.RoomProvisioned, ConsumerName)
    {
    }

    protected override async Task HandleAsync(IServiceProvider services, EventEnvelope<RoomProvisionedEvent> envelope, CancellationToken cancellationToken)
    {
        if (!envelope.Data.IsPublic)
        {
            return;
        }

        var roomRegistry = services.GetRequiredService<IRoomRegistry>();
        await roomRegistry.AddPublicRoomAsync(envelope.Data.RoomId);
    }
}
