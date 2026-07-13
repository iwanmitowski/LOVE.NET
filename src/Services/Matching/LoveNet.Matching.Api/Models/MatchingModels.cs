using System.ComponentModel.DataAnnotations;
using LoveNet.BuildingBlocks.Web;

namespace LoveNet.Matching.Api.Models;

/// <summary>Card/match shape the SPA renders (same contract as the monolith's UserMatchViewModel).</summary>
public class UserMatchModel
{
    public string Id { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string? Bio { get; set; }

    public DateTime Birthdate { get; set; }

    public int Age => DateHelper.CalculateAge(Birthdate);

    public List<ImageModel> Images { get; set; } = new();

    public int GenderId { get; set; }

    public string? GenderName { get; set; }

    public int CityId { get; set; }

    public string? CityName { get; set; }

    public int CountryId { get; set; }

    public string? CountryName { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? RoomId { get; set; }
}

/// <summary>Synthesized from the read model's image URLs; ids are positional.</summary>
public class ImageModel
{
    public int Id { get; set; }

    public string Url { get; set; } = null!;

    public bool IsProfilePicture { get; set; }
}

public class MatchesRequestModel
{
    [Required]
    public string UserId { get; set; } = null!;

    public string? Search { get; set; }

    [Required]
    public int Page { get; set; }
}

public class MatchesResponseModel
{
    public IEnumerable<UserMatchModel> Matches { get; set; } = Enumerable.Empty<UserMatchModel>();

    public int TotalMatches { get; set; }
}

public class MatchResponseModel
{
    public bool IsMatch { get; set; }

    public UserMatchModel? User { get; set; }
}
