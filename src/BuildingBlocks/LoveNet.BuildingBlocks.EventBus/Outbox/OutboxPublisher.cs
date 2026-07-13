using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoveNet.BuildingBlocks.EventBus.Outbox;

/// <summary>
/// Polls the host service's outbox table and relays pending rows to Kafka in
/// OccurredOn order. Stops the batch on the first failure so per-key ordering
/// is never violated by skipping ahead. Designed for a single publisher
/// instance per service; scaling out would require row locking.
/// </summary>
public class OutboxPublisher<TContext> : BackgroundService
    where TContext : DbContext
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(500);
    private const int BatchSize = 50;

    private readonly IServiceScopeFactory scopeFactory;
    private readonly IProducer<string, string> producer;
    private readonly ILogger<OutboxPublisher<TContext>> logger;

    public OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        ILogger<OutboxPublisher<TContext>> logger)
    {
        this.scopeFactory = scopeFactory;
        this.producer = producer;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox polling iteration failed");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();

        var pending = await db.Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        foreach (var message in pending)
        {
            try
            {
                await producer.ProduceAsync(
                    message.Topic,
                    new Message<string, string> { Key = message.Key, Value = message.Payload },
                    cancellationToken);

                message.ProcessedOn = DateTime.UtcNow;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message.Attempts++;
                message.LastError = ex.Message;
                logger.LogError(ex, "Failed to publish outbox message {MessageId} to {Topic} (attempt {Attempts})", message.Id, message.Topic, message.Attempts);
                break;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
