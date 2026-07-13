namespace LoveNet.Chat.Api.Data;

/// <summary>
/// Chat message. The physical table is range-partitioned by CreatedOn
/// (monthly) — hence the composite key (Id, CreatedOn); Postgres requires the
/// partition key inside the primary key.
/// </summary>
public class Message
{
    /// <summary>Assigned by the Realtime hub; doubles as the idempotency key.</summary>
    public string Id { get; set; } = null!;

    public string RoomId { get; set; } = null!;

    public string SenderId { get; set; } = null!;

    public string? Text { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedOn { get; set; }
}
