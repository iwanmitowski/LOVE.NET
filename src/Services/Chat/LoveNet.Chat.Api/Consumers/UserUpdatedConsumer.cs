using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Chat.Api.Data;
using Microsoft.Extensions.Options;

namespace LoveNet.Chat.Api.Consumers;

/// <summary>Keeps sender names/avatars current for chat-history rendering.</summary>
public class UserUpdatedConsumer : KafkaConsumerService<UserUpdatedEvent>
{
    private const string ConsumerName = "chat-service";

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
        var db = services.GetRequiredService<ChatDbContext>();

        if (!await db.TryRegisterInboxAsync(envelope.MessageId, ConsumerName, cancellationToken))
        {
            return;
        }

        var data = envelope.Data;
        var profile = await db.UserProfiles.FindAsync(new object[] { data.UserId }, cancellationToken);

        if (profile is null)
        {
            profile = new UserProfileLite { UserId = data.UserId };
            db.UserProfiles.Add(profile);
        }

        profile.UserName = data.UserName;
        profile.AvatarUrl = data.AvatarUrl;

        await db.SaveChangesAsync(cancellationToken);
    }
}
