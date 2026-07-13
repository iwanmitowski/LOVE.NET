namespace LoveNet.Matching.Api.Data;

/// <summary>
/// Local read model of user profiles, maintained from profile.user-updated
/// events. Lets the swipe deck and match list render without calling Profile.
/// </summary>
public class CandidateProfile
{
    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Bio { get; set; }

    public DateTime Birthdate { get; set; }

    public int GenderId { get; set; }

    public string? GenderName { get; set; }

    public int CityId { get; set; }

    public string? CityName { get; set; }

    public int CountryId { get; set; }

    public string? CountryName { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? AvatarUrl { get; set; }

    /// <summary>Profile-picture first (EF Core 8 primitive collection).</summary>
    public List<string> ImageUrls { get; set; } = new();

    public bool IsBanned { get; set; }

    public bool IsAdmin { get; set; }
}
