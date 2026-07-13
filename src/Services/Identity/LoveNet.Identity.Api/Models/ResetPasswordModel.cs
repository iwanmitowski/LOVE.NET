using System.ComponentModel.DataAnnotations;

namespace LoveNet.Identity.Api.Models;

public class ResetPasswordModel
{
    [Required]
    public string Token { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = null!;

    [Required]
    [MinLength(5)]
    public string ConfirmPassword { get; set; } = null!;
}
