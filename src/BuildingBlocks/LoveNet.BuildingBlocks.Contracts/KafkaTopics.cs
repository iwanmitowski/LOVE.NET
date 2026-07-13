namespace LoveNet.BuildingBlocks.Contracts;

public static class KafkaTopics
{
    public const string UserRegistered = "identity.user-registered";
    public const string UserBanned = "identity.user-banned";
    public const string UserUpdated = "profile.user-updated";
    public const string MatchCreated = "matching.match-created";
    public const string MessageSent = "chat.message-sent";
    public const string RoomProvisioned = "chat.room-provisioned";
    public const string RoomRead = "chat.room-read";
    public const string EmailRequested = "notifications.email-requested";

    /// <summary>Poison messages land here after bounded consumer retries.</summary>
    public const string DeadLetter = "dead-letter";
}
