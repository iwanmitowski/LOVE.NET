using System.Text.Json.Serialization;

namespace LoveNet.Identity.Api.Data;

public class RefreshToken
{
    [JsonIgnore]
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime Expires { get; set; }

    public bool IsExpired => DateTime.UtcNow >= Expires;

    public DateTime? Revoked { get; set; }

    public bool IsActive => Revoked == null && !IsExpired;

    public DateTime CreatedOn { get; set; }

    public string? ReplacedByToken { get; set; }
}
