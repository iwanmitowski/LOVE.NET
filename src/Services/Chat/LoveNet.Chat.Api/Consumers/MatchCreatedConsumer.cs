using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Chat.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LoveNet.Chat.Api.Consumers;

/// <summary>Provisions the 1:1 room for a new match and announces it to Realtime.</summary>
public class MatchCreatedConsumer : KafkaConsumerService<MatchCreatedEvent>
{
    private const string ConsumerName = "chat-service";

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
        var db = services.GetRequiredService<ChatDbContext>();

        if (!await db.TryRegisterInboxAsync(envelope.MessageId, ConsumerName, cancellationToken))
        {
            return;
        }

        var data = envelope.Data;

        if (!await db.Rooms.AnyAsync(r => r.RoomId == data.RoomId, cancellationToken))
        {
            db.Rooms.Add(new Room
            {
                RoomId = data.RoomId,
                UserAId = data.UserA.Id,
                UserBId = data.UserB.Id,
                CreatedOn = data.CreatedOn,
            });

            db.AddOutboxMessage(
                KafkaTopics.RoomProvisioned,
                data.RoomId,
                new RoomProvisionedEvent(
                    data.RoomId,
                    IsPublic: false,
                    new[] { data.UserA.Id, data.UserB.Id },
                    Name: null),
                envelope.CorrelationId);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
