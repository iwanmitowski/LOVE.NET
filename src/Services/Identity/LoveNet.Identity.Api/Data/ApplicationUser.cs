using Microsoft.AspNetCore.Identity;

namespace LoveNet.Identity.Api.Data;

/// <summary>
/// Credentials-only user: profile attributes (bio, geo, images) live in the
/// Profile service and reach it through the identity.user-registered event.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid().ToString();
    }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
}
