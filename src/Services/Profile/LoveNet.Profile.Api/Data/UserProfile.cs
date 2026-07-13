using System.ComponentModel.DataAnnotations;

namespace LoveNet.Profile.Api.Data;

/// <summary>
/// Profile aggregate owned by this service. The user id comes from Identity
/// via the identity.user-registered event and is shared across all services.
/// </summary>
public class UserProfile
{
    public string UserId { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = null!;

    [MaxLength(255)]
    public string? Bio { get; set; }

    public DateTime Birthdate { get; set; }

    public int GenderId { get; set; }

    public virtual Gender Gender { get; set; } = null!;

    public int CountryId { get; set; }

    public virtual Country Country { get; set; } = null!;

    public int CityId { get; set; }

    public virtual City City { get; set; } = null!;

    /// <summary>Mirrors Identity's lockout via identity.user-banned events.</summary>
    public bool IsBanned { get; set; }

    /// <summary>Mirrors the Administrator role; downstream services exclude admins from the deck.</summary>
    public bool IsAdmin { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new HashSet<Image>();
}
