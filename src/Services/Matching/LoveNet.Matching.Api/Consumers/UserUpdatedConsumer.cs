using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.EventBus.Consuming;
using LoveNet.Matching.Api.Data;
using Microsoft.Extensions.Options;

namespace LoveNet.Matching.Api.Consumers;

/// <summary>Maintains the candidate_profiles read model for the swipe deck.</summary>
public class UserUpdatedConsumer : KafkaConsumerService<UserUpdatedEvent>
{
    private const string ConsumerName = "matching-service";

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
        var db = services.GetRequiredService<MatchingDbContext>();

        if (!await db.TryRegisterInboxAsync(envelope.MessageId, ConsumerName, cancellationToken))
        {
            return;
        }

        var data = envelope.Data;
        var candidate = await db.CandidateProfiles.FindAsync(new object[] { data.UserId }, cancellationToken);

        if (candidate is null)
        {
            candidate = new CandidateProfile { UserId = data.UserId };
            db.CandidateProfiles.Add(candidate);
        }

        candidate.UserName = data.UserName;
        candidate.Email = data.Email;
        candidate.Bio = data.Bio;
        candidate.Birthdate = data.Birthdate;
        candidate.GenderId = data.GenderId;
        candidate.GenderName = data.GenderName;
        candidate.CityId = data.CityId;
        candidate.CityName = data.CityName;
        candidate.CountryId = data.CountryId;
        candidate.CountryName = data.CountryName;
        candidate.Latitude = data.Latitude ?? 0;
        candidate.Longitude = data.Longitude ?? 0;
        candidate.AvatarUrl = data.AvatarUrl;
        candidate.ImageUrls = data.ImageUrls.ToList();
        candidate.IsBanned = data.IsBanned;
        candidate.IsAdmin = data.IsAdmin;

        await db.SaveChangesAsync(cancellationToken);
    }
}
