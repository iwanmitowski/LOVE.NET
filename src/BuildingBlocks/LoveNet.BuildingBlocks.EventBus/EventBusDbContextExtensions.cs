using System.Text.Json;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.EventBus.Inbox;
using LoveNet.BuildingBlocks.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.BuildingBlocks.EventBus;

public static class EventBusDbContextExtensions
{
    /// <summary>Maps the outbox_messages table. Call from OnModelCreating.</summary>
    public static ModelBuilder AddOutbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => m.ProcessedOn);
            entity.Property(m => m.Topic).HasMaxLength(128);
            entity.Property(m => m.Key).HasMaxLength(128);
        });

        return modelBuilder;
    }

    /// <summary>Maps the inbox_messages table. Call from OnModelCreating.</summary>
    public static ModelBuilder AddInbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.ToTable("inbox_messages");
            entity.HasKey(m => new { m.MessageId, m.Consumer });
            entity.Property(m => m.Consumer).HasMaxLength(128);
        });

        return modelBuilder;
    }

    /// <summary>
    /// Queues an event in the outbox as part of the current unit of work.
    /// Becomes visible to the publisher only when SaveChanges commits.
    /// </summary>
    public static void AddOutboxMessage<T>(this DbContext db, string topic, string key, T data, string? correlationId = null)
    {
        var envelope = EventEnvelope<T>.Create(data, correlationId);

        db.Set<OutboxMessage>().Add(new OutboxMessage
        {
            Id = envelope.MessageId,
            Topic = topic,
            Key = key,
            Payload = JsonSerializer.Serialize(envelope, JsonDefaults.Options),
            OccurredOn = envelope.OccurredOn,
        });
    }

    /// <summary>
    /// Returns false when the envelope was already processed by this consumer.
    /// Otherwise registers it; the row commits atomically with the handler's
    /// SaveChanges, and a concurrent duplicate fails on the composite PK.
    /// </summary>
    public static async Task<bool> TryRegisterInboxAsync(this DbContext db, Guid messageId, string consumer, CancellationToken cancellationToken = default)
    {
        var alreadyProcessed = await db.Set<InboxMessage>()
            .AnyAsync(m => m.MessageId == messageId && m.Consumer == consumer, cancellationToken);

        if (alreadyProcessed)
        {
            return false;
        }

        db.Set<InboxMessage>().Add(new InboxMessage
        {
            MessageId = messageId,
            Consumer = consumer,
            ProcessedOn = DateTime.UtcNow,
        });

        return true;
    }
}
