namespace LoveNet.BuildingBlocks.EventBus;

/// <summary>
/// Direct (non-outbox) publish. Use only from services without a database of
/// their own (e.g. Realtime); everywhere else prefer the transactional outbox.
/// </summary>
public interface IEventPublisher
{
    /// <summary>Awaits broker acknowledgement (acks=all) before returning.</summary>
    Task PublishAsync<T>(string topic, string key, T data, string? correlationId = null, CancellationToken cancellationToken = default);
}
