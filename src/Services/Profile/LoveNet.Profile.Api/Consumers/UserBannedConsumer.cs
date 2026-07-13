using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Profile.Api.Data;
using LoveNet.Profile.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LoveNet.Profile.Api.Consumers;

public class UserBannedConsumer : KafkaConsumerService<UserBannedEvent>
{
    private const string ConsumerName = "profile-service";

    public UserBannedConsumer(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger<UserBannedConsumer> logger)
        : base(scopeFactory, producer, options, logger, KafkaTopics.UserBanned, ConsumerName)
    {
    }

    protected override async Task HandleAsync(IServiceProvider services, EventEnvelope<UserBannedEvent> envelope, CancellationToken cancellationToken)
    {
        var db = services.GetRequiredService<ProfileDbContext>();

        if (!await db.TryRegisterInboxAsync(envelope.MessageId, ConsumerName, cancellationToken))
        {
            return;
        }

        var profile = await db.UserProfiles
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.UserId == envelope.Data.UserId, cancellationToken);

        if (profile is null)
        {
            return;
        }

        profile.IsBanned = envelope.Data.BannedUntil is not null;

        var publisher = services.GetRequiredService<IUserUpdatedPublisher>();
        await publisher.QueueUserUpdatedAsync(profile, envelope.CorrelationId);

        await db.SaveChangesAsync(cancellationToken);
    }
}
