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
    /// <summary>Server-assigned message id (assigned by the Realtime hub).</summary>
    public string Id { get; set; } = null!;

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

public class SummariesRequestModel
{
    public List<string> RoomIds { get; set; } = new();
}

/// <summary>Conversation-list data for one room: last message + unread state.</summary>
public class RoomSummaryModel
{
    public string RoomId { get; set; } = null!;

    public string? LastMessageText { get; set; }

    public string? LastMessageImageUrl { get; set; }

    public string? LastMessageSenderId { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public int UnreadCount { get; set; }

    /// <summary>When the other 1:1 participant last read the room (null for public rooms).</summary>
    public DateTime? PeerLastReadAt { get; set; }
}
