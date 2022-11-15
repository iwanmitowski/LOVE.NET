namespace LOVE.NET.Web.ViewModels.Identity
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.Infrastructure.Attributes;
    using Microsoft.AspNetCore.Http;

    using static LOVE.NET.Common.GlobalConstants;

    public class RegisterViewModel : BaseCredentialsModel, IMapTo<ApplicationUser>
    {
        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(255)]
        public string Bio { get; set; }

        public DateTime Birthdate { get; set; }

        public int CountryId { get; set; }

        public int GenderId { get; set; }

        public int CityId { get; set; }

        [MaxFileSize(MaxFileSizeInBytes)]
        [AllowedFileExtensions(new string[] { ".jpg", ".png" })]
        public IFormFile Image { get; set; }

        [MaxFileSize(MaxFileSizeInBytes)]
        [AllowedFileExtensions(new string[] { ".jpg", ".png" })]
        public IFormFile[] NewPhotos { get; set; }
    }
}
