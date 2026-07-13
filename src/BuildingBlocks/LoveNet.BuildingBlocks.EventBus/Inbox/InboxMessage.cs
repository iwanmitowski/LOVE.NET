namespace LoveNet.BuildingBlocks.EventBus.Inbox;

/// <summary>
/// Consumer-side dedupe record: one row per processed envelope MessageId.
/// Inserted in the same transaction as the event's side effects, so a
/// redelivered message either sees the row (skip) or conflicts on the PK.
/// </summary>
public class InboxMessage
{
    public Guid MessageId { get; set; }

    public string Consumer { get; set; } = null!;

    public DateTime ProcessedOn { get; set; }
}
