namespace LoveNet.BuildingBlocks.Contracts;

/// <summary>
/// Wire format for every event on Kafka. <see cref="MessageId"/> is the
/// idempotency key consumers dedupe on; <see cref="CorrelationId"/> carries
/// the originating request id across service boundaries.
/// </summary>
public record EventEnvelope<T>(
    Guid MessageId,
    string Type,
    int Version,
    DateTime OccurredOn,
    string? CorrelationId,
    T Data)
{
    public static EventEnvelope<T> Create(T data, string? correlationId = null) =>
        new(
            Guid.NewGuid(),
            typeof(T).Name,
            1,
            DateTime.UtcNow,
            correlationId,
            data);
}
