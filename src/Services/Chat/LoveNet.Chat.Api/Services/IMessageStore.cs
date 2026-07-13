using LoveNet.Chat.Api.Data;
using LoveNet.Chat.Api.Models;

namespace LoveNet.Chat.Api.Services;

/// <summary>
/// Storage seam for chat messages. The default implementation is Postgres
/// (monthly range partitions); the interface is deliberately narrow —
/// append + page-by-room — so a Cassandra/ScyllaDB implementation keyed on
/// (room_id, time_bucket) can be swapped in without touching callers.
/// </summary>
public interface IMessageStore
{
    /// <summary>Idempotent by message id: appending the same id twice is a no-op.</summary>
    Task AppendAsync(Message message, CancellationToken cancellationToken = default);

    Task<ChatResponseModel> GetPageAsync(string roomId, int page, CancellationToken cancellationToken = default);
}
