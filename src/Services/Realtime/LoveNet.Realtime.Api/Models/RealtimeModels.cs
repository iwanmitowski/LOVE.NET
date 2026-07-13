namespace LoveNet.Realtime.Api.Models;

/// <summary>
/// Join/leave payload. The SPA still sends the legacy UserConnection shape
/// with userId/userName; those extra JSON fields are ignored on binding —
/// identity comes from JWT claims. ProfilePictureUrl is cosmetic only.
/// </summary>
public class UserConnectionModel
{
    public string RoomId { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }
}

public class SendMessageModel
{
    public string RoomId { get; set; } = string.Empty;

    public string? Text { get; set; }

    public string? ImageUrl { get; set; }

    public string? ProfilePicture { get; set; }
}

public class UserInRoomModel
{
    public string Id { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }

    public string? UserName { get; set; }
}
