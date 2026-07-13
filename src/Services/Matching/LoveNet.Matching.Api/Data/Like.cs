namespace LoveNet.Matching.Api.Data;

/// <summary>One row per directed like; a reciprocal pair creates a Match.</summary>
public class Like
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string LikedUserId { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
}
