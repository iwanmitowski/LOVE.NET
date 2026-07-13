using System.ComponentModel.DataAnnotations;

namespace LoveNet.Identity.Api.Models;

public class LoginModel
{
    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = null!;
}
