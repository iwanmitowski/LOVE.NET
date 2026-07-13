using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Chat.Api.Data;
using Microsoft.Extensions.Options;

namespace LoveNet.Chat.Api.Consumers;

/// <summary>
/// Persists last-read watermarks published by the Realtime hub. Naturally
/// idempotent — the watermark only moves forward — so no inbox row is needed.
/// </summary>
public class RoomReadConsumer : KafkaConsumerService<RoomReadEvent>
{
    private const string ConsumerName = "chat-service";

    public RoomReadConsumer(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger<RoomReadConsumer> logger)
        : base(scopeFactory, producer, options, logger, KafkaTopics.RoomRead, ConsumerName)
    {
    }

    protected override async Task HandleAsync(IServiceProvider services, EventEnvelope<RoomReadEvent> envelope, CancellationToken cancellationToken)
    {
        var db = services.GetRequiredService<ChatDbContext>();
        var data = envelope.Data;

        var roomRead = await db.RoomReads.FindAsync(new object[] { data.RoomId, data.UserId }, cancellationToken);

        if (roomRead is null)
        {
            db.RoomReads.Add(new RoomRead
            {
                RoomId = data.RoomId,
                UserId = data.UserId,
                LastReadAt = data.ReadAt,
            });
        }
        else if (data.ReadAt > roomRead.LastReadAt)
        {
            roomRead.LastReadAt = data.ReadAt;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
