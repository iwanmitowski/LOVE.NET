using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.Profile.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Profile.Api.Services;

public interface IUserUpdatedPublisher
{
    /// <summary>
    /// Queues an enriched profile.user-updated outbox event reflecting the
    /// (possibly not yet saved) state of the given profile. The caller's
    /// SaveChanges commits event and state together.
    /// </summary>
    Task QueueUserUpdatedAsync(UserProfile profile, string? correlationId);
}

public class UserUpdatedPublisher : IUserUpdatedPublisher
{
    private readonly ProfileDbContext db;

    public UserUpdatedPublisher(ProfileDbContext db)
    {
        this.db = db;
    }

    public async Task QueueUserUpdatedAsync(UserProfile profile, string? correlationId)
    {
        var gender = await db.Genders.AsNoTracking().FirstOrDefaultAsync(g => g.Id == profile.GenderId);
        var city = await db.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == profile.CityId);
        var country = await db.Countries.AsNoTracking().FirstOrDefaultAsync(c => c.Id == profile.CountryId);

        var visibleImages = profile.Images
            .Where(i => !i.IsDeleted)
            .OrderByDescending(i => i.IsProfilePicture)
            .ToList();

        var payload = new UserUpdatedEvent(
            profile.UserId,
            profile.Email,
            profile.UserName,
            profile.Bio,
            profile.Birthdate,
            profile.GenderId,
            gender?.Name,
            profile.CityId,
            city?.Name,
            city?.Latitude,
            city?.Longitude,
            profile.CountryId,
            country?.Name,
            visibleImages.FirstOrDefault(i => i.IsProfilePicture)?.Url ?? visibleImages.FirstOrDefault()?.Url,
            visibleImages.Select(i => i.Url).ToList(),
            profile.IsBanned,
            profile.IsAdmin);

        db.AddOutboxMessage(KafkaTopics.UserUpdated, profile.UserId, payload, correlationId);
    }
}
