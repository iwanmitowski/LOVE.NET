namespace LOVE.NET.Web.ViewModels.Identity
{
    using System.ComponentModel.DataAnnotations;

    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [MinLength(5)]
        public string Password { get; set; }

        [Required]
        [MinLength(5)]
        public string ConfirmPassword { get; set; }
    }
}
