namespace LoveNet.Chat.Api.Data;

/// <summary>Public/group room, seeded from Chatrooms.json (monolith parity).</summary>
public class Chatroom
{
    public string Id { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Url { get; set; } = null!;
}
