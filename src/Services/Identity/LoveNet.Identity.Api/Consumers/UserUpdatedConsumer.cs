using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Identity.Api.Data;
using Microsoft.Extensions.Options;

namespace LoveNet.Identity.Api.Consumers;

/// <summary>
/// Maintains the lite profile read model that enriches the login response.
/// </summary>
public class UserUpdatedConsumer : KafkaConsumerService<UserUpdatedEvent>
{
    private const string ConsumerName = "identity-service";

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
        var db = services.GetRequiredService<AppIdentityDbContext>();

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

        profile.Bio = data.Bio;
        profile.Birthdate = data.Birthdate;
        profile.CountryName = data.CountryName;
        profile.CityName = data.CityName;
        profile.Latitude = data.Latitude;
        profile.Longitude = data.Longitude;
        profile.AvatarUrl = data.AvatarUrl;

        await db.SaveChangesAsync(cancellationToken);
    }
}
