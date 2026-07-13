namespace LoveNet.BuildingBlocks.Contracts.Events;

/// <summary>Published by Identity when registration succeeds. Key: userId.</summary>
public record UserRegisteredEvent(
    string UserId,
    string Email,
    string UserName,
    string? Bio,
    DateTime Birthdate,
    int GenderId,
    int CityId,
    int CountryId,
    IReadOnlyList<string> ImageUrls);

/// <summary>Published by Identity on moderation. BannedUntil null = unbanned. Key: userId.</summary>
public record UserBannedEvent(
    string UserId,
    DateTimeOffset? BannedUntil);
