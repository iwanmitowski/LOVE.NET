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

/// <summary>
/// Builds the profile aggregate when Identity registers a user, then queues
/// the enriched profile.user-updated event for the other services.
/// </summary>
public class UserRegisteredConsumer : KafkaConsumerService<UserRegisteredEvent>
{
    private const string ConsumerName = "profile-service";

    public UserRegisteredConsumer(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger<UserRegisteredConsumer> logger)
        : base(scopeFactory, producer, options, logger, KafkaTopics.UserRegistered, ConsumerName)
    {
    }

    protected override async Task HandleAsync(IServiceProvider services, EventEnvelope<UserRegisteredEvent> envelope, CancellationToken cancellationToken)
    {
        var db = services.GetRequiredService<ProfileDbContext>();

        if (!await db.TryRegisterInboxAsync(envelope.MessageId, ConsumerName, cancellationToken))
        {
            return;
        }

        var data = envelope.Data;

        var profile = await db.UserProfiles
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.UserId == data.UserId, cancellationToken);

        if (profile is null)
        {
            profile = new UserProfile
            {
                UserId = data.UserId,
                CreatedOn = DateTime.UtcNow,
            };
            db.UserProfiles.Add(profile);
        }

        profile.UserName = data.UserName;
        profile.Email = data.Email;
        profile.Bio = data.Bio;
        profile.Birthdate = data.Birthdate;
        profile.GenderId = data.GenderId;
        profile.CityId = data.CityId;
        profile.CountryId = data.CountryId;
        profile.IsAdmin = data.IsAdmin;

        if (profile.Images.Count == 0)
        {
            var isFirst = true;

            foreach (var url in data.ImageUrls)
            {
                profile.Images.Add(new Image
                {
                    Url = url,
                    IsProfilePicture = isFirst,
                    CreatedOn = DateTime.UtcNow,
                });
                isFirst = false;
            }
        }

        var publisher = services.GetRequiredService<IUserUpdatedPublisher>();
        await publisher.QueueUserUpdatedAsync(profile, envelope.CorrelationId);

        await db.SaveChangesAsync(cancellationToken);
    }
}
