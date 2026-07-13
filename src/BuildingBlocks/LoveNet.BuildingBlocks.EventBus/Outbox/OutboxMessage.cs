namespace LoveNet.BuildingBlocks.EventBus.Outbox;

/// <summary>
/// Transactional outbox row. Written in the same transaction as the business
/// change; <see cref="OutboxPublisher{TContext}"/> relays it to Kafka.
/// </summary>
public class OutboxMessage
{
    /// <summary>Equals the envelope's MessageId.</summary>
    public Guid Id { get; set; }

    public string Topic { get; set; } = null!;

    public string Key { get; set; } = null!;

    /// <summary>The full serialized <c>EventEnvelope&lt;T&gt;</c>.</summary>
    public string Payload { get; set; } = null!;

    public DateTime OccurredOn { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public int Attempts { get; set; }

    public string? LastError { get; set; }
}
