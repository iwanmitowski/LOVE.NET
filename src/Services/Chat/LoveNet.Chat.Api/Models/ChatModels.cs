using System.ComponentModel.DataAnnotations;

namespace LoveNet.Chat.Api.Models;

public class ChatRequestModel
{
    [Required]
    public string RoomId { get; set; } = null!;

    [Required]
    public int Page { get; set; }
}

/// <summary>History item — same field names the SPA renders (monolith MessageDto).</summary>
public class MessageModel
{
    public string RoomId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string? Text { get; set; }

    /// <summary>Current profile picture of the sender (joined from the read model).</summary>
    public string? ProfilePicture { get; set; }

    /// <summary>Image sent as the message body.</summary>
    public string? ImageUrl { get; set; }

    public DateTime CreatedOn { get; set; }
}

public class ChatResponseModel
{
    public IEnumerable<MessageModel> Messages { get; set; } = Enumerable.Empty<MessageModel>();

    public int TotalMessages { get; set; }
}

public class ChatroomModel
{
    public string Id { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Url { get; set; } = null!;
}
