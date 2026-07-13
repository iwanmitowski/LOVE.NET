namespace LoveNet.BuildingBlocks.Contracts.Events;

/// <summary>
/// Published by Profile whenever a profile is created or changes. Enriched with
/// geo names/coordinates and image URLs so consumers need no reference-data joins.
/// Key: userId.
/// </summary>
public record UserUpdatedEvent(
    string UserId,
    string Email,
    string UserName,
    string? Bio,
    DateTime Birthdate,
    int GenderId,
    string? GenderName,
    int CityId,
    string? CityName,
    double? Latitude,
    double? Longitude,
    int CountryId,
    string? CountryName,
    string? AvatarUrl,
    IReadOnlyList<string> ImageUrls,
    bool IsBanned,
    bool IsAdmin = false);
