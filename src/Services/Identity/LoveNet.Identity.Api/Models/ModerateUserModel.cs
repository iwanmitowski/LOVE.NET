namespace LoveNet.Identity.Api.Models;

public class ModerateUserModel
{
    public string UserId { get; set; } = null!;

    /// <summary>Lockout end; null lifts the ban.</summary>
    public DateTimeOffset? BannedUntil { get; set; }
}
