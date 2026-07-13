namespace LoveNet.Identity.Api.Models;

/// <summary>
/// Same shape as the monolith's LoginResponseModel — the SPA persists this in
/// localStorage. Profile fields come from the local read model (see
/// UserProfileLite), which is eventually consistent with the Profile service.
/// </summary>
public class LoginResponseModel
{
    public string Id { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public bool IsAdmin { get; set; }

    public string? Bio { get; set; }

    public DateTime Birthdate { get; set; }

    public string? CountryName { get; set; }

    public string? CityName { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? ProfilePicture { get; set; }
}
