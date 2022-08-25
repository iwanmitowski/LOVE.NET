﻿namespace LOVE.NET.Web.ViewModels.Identity
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

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

        public int CityId { get; set; }
    }
}
