namespace LoveNet.Chat.Api.Data;

/// <summary>
/// Last-read watermark per (room, user) — the basis for unread counts and
/// read receipts. Deliberately not per-message rows: the partitioned messages
/// table has a composite PK that makes message-level FKs painful, and a
/// watermark is all a messenger UI needs.
/// </summary>
public class RoomRead
{
    public string RoomId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateTime LastReadAt { get; set; }
}
