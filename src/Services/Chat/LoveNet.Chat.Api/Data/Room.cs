namespace LoveNet.Chat.Api.Data;

/// <summary>
/// 1:1 room provisioned from matching.match-created. Hardens authorization
/// beyond the deterministic roomId format the ids are derived from.
/// </summary>
public class Room
{
    public string RoomId { get; set; } = null!;

    public string UserAId { get; set; } = null!;

    public string UserBId { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
}
