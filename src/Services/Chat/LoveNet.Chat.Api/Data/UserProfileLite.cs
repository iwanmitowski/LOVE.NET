namespace LoveNet.Chat.Api.Data;

/// <summary>Read model from profile.user-updated; joined into chat history so avatars stay current.</summary>
public class UserProfileLite
{
    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string? AvatarUrl { get; set; }
}
