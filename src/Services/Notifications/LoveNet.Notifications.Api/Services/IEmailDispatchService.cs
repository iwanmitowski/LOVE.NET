using LoveNet.BuildingBlocks.Contracts.Events;

namespace LoveNet.Notifications.Api.Services;

public interface IEmailDispatchService
{
    Task HandleEmailRequestedAsync(Guid messageId, EmailRequestedEvent evt, CancellationToken ct);

    Task HandleMatchCreatedAsync(Guid messageId, MatchCreatedEvent evt, CancellationToken ct);

    Task UpsertContactAsync(Guid messageId, string userId, string email, string userName, CancellationToken ct);
}
