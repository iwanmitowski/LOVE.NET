namespace LoveNet.Realtime.Api.Models;

/// <summary>
/// Join/leave payload (legacy UserConnection shape). UserId and UserName are
/// ignored server-side — identity comes from JWT claims; only
/// ProfilePictureUrl is trusted, as cosmetic data.
/// </summary>
public class UserConnectionModel
{
    public string? UserId { get; set; }

    public string RoomId { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }

    public string? UserName { get; set; }
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
