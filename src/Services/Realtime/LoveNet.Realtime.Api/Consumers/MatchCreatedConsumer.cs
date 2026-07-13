using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Realtime.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace LoveNet.Realtime.Api.Consumers;

/// <summary>
/// Pushes a "MatchReceived" toast to both matched users. No inbox by design:
/// redelivery only repeats a toast and the client dedupes.
/// </summary>
public class MatchCreatedConsumer : KafkaConsumerService<MatchCreatedEvent>
{
    private const string ConsumerName = "realtime-service";

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
        var hub = services.GetRequiredService<IHubContext<ChatHub>>();
        var data = envelope.Data;

        await hub.Clients.User(data.UserA.Id).SendAsync(
            "MatchReceived",
            new { data.MatchId, data.RoomId, User = data.UserB },
            cancellationToken);

        await hub.Clients.User(data.UserB.Id).SendAsync(
            "MatchReceived",
            new { data.MatchId, data.RoomId, User = data.UserA },
            cancellationToken);
    }
}
