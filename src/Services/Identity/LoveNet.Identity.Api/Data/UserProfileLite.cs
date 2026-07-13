namespace LoveNet.Identity.Api.Data;

/// <summary>
/// Read model kept in sync from profile.user-updated events so the login
/// response can carry the profile fields the SPA stores (avatar, location).
/// </summary>
public class UserProfileLite
{
    public string UserId { get; set; } = null!;

    public string? Bio { get; set; }

    public DateTime Birthdate { get; set; }

    public string? CountryName { get; set; }

    public string? CityName { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? AvatarUrl { get; set; }
}
