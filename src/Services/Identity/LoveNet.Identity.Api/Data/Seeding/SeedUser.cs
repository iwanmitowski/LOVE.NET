namespace LoveNet.Identity.Api.Data.Seeding;

/// <summary>Shape of Data/Files/Users.json (carried over from the monolith).</summary>
public class SeedUser
{
    public DateTime CreatedOn { get; set; }

    public int GenderId { get; set; }

    public string Bio { get; set; } = null!;

    public DateTime Birthdate { get; set; }

    public List<SeedUserImage> Images { get; set; } = new();

    public int CountryId { get; set; }

    public int CityId { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string NormalizedEmail { get; set; } = null!;

    public bool EmailConfirmed { get; set; }

    public string PasswordHash { get; set; } = null!;

    public bool LockoutEnabled { get; set; }
}

public class SeedUserImage
{
    public string Url { get; set; } = null!;

    public bool IsProfilePicture { get; set; }
}
