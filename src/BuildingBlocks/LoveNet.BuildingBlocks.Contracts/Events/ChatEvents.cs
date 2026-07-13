namespace LoveNet.BuildingBlocks.Contracts.Events;

/// <summary>
/// Published by Realtime after a hub client sends a message; consumed by Chat
/// for persistence. Id is assigned server-side in the hub and doubles as the
/// idempotency key. Key: roomId (per-room ordering).
/// </summary>
public record MessageSentEvent(
    string Id,
    string RoomId,
    string SenderId,
    string SenderUserName,
    string? SenderAvatarUrl,
    string? Text,
    string? ImageUrl,
    DateTime CreatedOn);

/// <summary>
/// Published by Chat when a room becomes joinable (match room or seeded public
/// chatroom); consumed by Realtime for join authorization. Key: roomId.
/// </summary>
public record RoomProvisionedEvent(
    string RoomId,
    bool IsPublic,
    IReadOnlyList<string> MemberIds,
    string? Name);
