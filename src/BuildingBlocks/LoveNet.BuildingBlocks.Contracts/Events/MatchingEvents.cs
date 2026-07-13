namespace LoveNet.BuildingBlocks.Contracts.Events;

public record MatchedUser(
    string Id,
    string UserName,
    string Email,
    string? AvatarUrl);

/// <summary>
/// Published by Matching when a reciprocal like creates a match.
/// RoomId is the deterministic 1:1 room id (the two user GUIDs sorted
/// ascending and concatenated). Key: roomId.
/// </summary>
public record MatchCreatedEvent(
    string MatchId,
    string RoomId,
    MatchedUser UserA,
    MatchedUser UserB,
    DateTime CreatedOn);
