using System.ComponentModel.DataAnnotations;
using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Validation;

namespace LoveNet.Profile.Api.Models;

/// <summary>Account response — same shape the SPA already consumes (minus the unused Matches list).</summary>
public class UserDetailsModel
{
    public string Id { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string? Bio { get; set; }

    public DateTime Birthdate { get; set; }

    public List<ImageModel> Images { get; set; } = new();

    public int GenderId { get; set; }

    public string? GenderName { get; set; }

    public int CityId { get; set; }

    public string? CityName { get; set; }

    public int CountryId { get; set; }

    public string? CountryName { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool IsBanned { get; set; }
}

public class ImageModel
{
    public int Id { get; set; }

    public string Url { get; set; } = null!;

    public bool IsProfilePicture { get; set; }

    public bool IsDeleted { get; set; }
}

/// <summary>Multipart account edit form; Images items are JSON-serialized ImageModel strings (SPA quirk kept).</summary>
public class EditUserModel
{
    [Required]
    public string Id { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Bio { get; set; } = null!;

    public DateTime Birthdate { get; set; }

    public int CountryId { get; set; }

    public int GenderId { get; set; }

    public int CityId { get; set; }

    public IEnumerable<string>? Images { get; set; }

    [MaxFileSize(LoveNetConstants.MaxFileSizeInBytes)]
    [AllowedFileExtensions(new[] { ".jpg", ".png" })]
    public IFormFile[]? NewImages { get; set; }
}

public class CountryModel
{
    public int CountryId { get; set; }

    public string CountryName { get; set; } = null!;
}

public class CityModel
{
    public int CityId { get; set; }

    public string CityName { get; set; } = null!;
}

public class CountryCitiesModel
{
    public int CountryId { get; set; }

    public string CountryName { get; set; } = null!;

    public List<CityModel> Cities { get; set; } = new();
}

public class GenderModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}

public class AdminUsersRequestModel
{
    public bool ShowBanned { get; set; }

    public string? Search { get; set; }

    public int Page { get; set; }
}

public class AdminUsersResponseModel
{
    public IEnumerable<UserDetailsModel> Users { get; set; } = Enumerable.Empty<UserDetailsModel>();

    public int TotalUsers { get; set; }
}
