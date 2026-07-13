namespace LoveNet.Matching.Api.Data;

/// <summary>
/// Persisted match (the monolith derived these on the fly). RoomId keeps the
/// legacy convention: both user ids sorted ascending, concatenated — Chat and
/// Realtime rely on that format for their fallback authorization.
/// </summary>
public class Match
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserAId { get; set; } = null!;

    public string UserBId { get; set; } = null!;

    public string RoomId { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public static string BuildRoomId(string userId, string otherUserId) =>
        string.Join(string.Empty, new[] { userId, otherUserId }.OrderBy(id => id));
}
