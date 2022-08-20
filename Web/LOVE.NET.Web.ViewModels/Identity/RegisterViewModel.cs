namespace LOVE.NET.Web.ViewModels.Identity
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterViewModel : BaseCredentialsModel
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

        [Range(18, 150)]
        public int Age { get; set; }

        public int CountryId { get; set; }

        public int CityId { get; set; }
    }
}
