namespace LOVE.NET.Web.ViewModels.Identity
{
    using System.ComponentModel.DataAnnotations;

    public class BaseCredentialsModel
    {

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string Password { get; set; }
    }
}
