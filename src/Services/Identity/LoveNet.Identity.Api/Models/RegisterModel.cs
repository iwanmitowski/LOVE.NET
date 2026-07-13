using System.ComponentModel.DataAnnotations;
using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Validation;

namespace LoveNet.Identity.Api.Models;

/// <summary>Multipart registration form — same field names the SPA already posts.</summary>
public class RegisterModel
{
    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = null!;

    [Required]
    [StringLength(50, MinimumLength = 5)]
    public string ConfirmPassword { get; set; } = null!;

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

    [MaxFileSize(LoveNetConstants.MaxFileSizeInBytes)]
    [AllowedFileExtensions(new[] { ".jpg", ".png" })]
    public IFormFile? Image { get; set; }

    [MaxFileSize(LoveNetConstants.MaxFileSizeInBytes)]
    [AllowedFileExtensions(new[] { ".jpg", ".png" })]
    public IFormFile[]? NewImages { get; set; }
}
