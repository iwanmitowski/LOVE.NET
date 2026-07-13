using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using LoveNet.BuildingBlocks.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoveNet.BuildingBlocks.EventBus.Consuming;

/// <summary>
/// Base class for one topic/one event-type Kafka consumers. Manual commits,
/// bounded retries with exponential backoff, then dead-letter and move on so
/// a poison message never stalls the partition.
/// </summary>
public abstract class KafkaConsumerService<TEvent> : BackgroundService
{
    private static readonly TimeSpan[] RetryDelays =
    {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
    };

    private readonly IServiceScopeFactory scopeFactory;
    private readonly IProducer<string, string> producer;
    private readonly KafkaOptions options;

    protected KafkaConsumerService(
        IServiceScopeFactory scopeFactory,
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger logger,
        string topic,
        string groupId)
    {
        this.scopeFactory = scopeFactory;
        this.producer = producer;
        this.options = options.Value;
        Logger = logger;
        Topic = topic;
        GroupId = groupId;
    }

    protected ILogger Logger { get; }

    protected string Topic { get; }

    protected string GroupId { get; }

    /// <summary>
    /// Handles one envelope inside a fresh DI scope. Implementations are
    /// responsible for idempotency (see TryRegisterInboxAsync).
    /// </summary>
    protected abstract Task HandleAsync(IServiceProvider services, EventEnvelope<TEvent> envelope, CancellationToken cancellationToken);

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(() => ConsumeLoopAsync(stoppingToken), stoppingToken);

    private async Task ConsumeLoopAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = options.BootstrapServers,
            GroupId = GroupId,
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AllowAutoCreateTopics = false,
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => Logger.LogWarning("Kafka consumer error on {Topic}: {Reason}", Topic, e.Reason))
            .Build();

        consumer.Subscribe(Topic);
        Logger.LogInformation("Consuming {Topic} as {GroupId}", Topic, GroupId);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? result = null;

            try
            {
                result = consumer.Consume(stoppingToken);
                await ProcessAsync(result, stoppingToken);
                consumer.Commit(result);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                Logger.LogError(ex, "Consume failed on {Topic}", Topic);
            }
            catch (Exception ex)
            {
                await DeadLetterAsync(result, ex, stoppingToken);
                if (result is not null)
                {
                    consumer.Commit(result);
                }
            }
        }

        consumer.Close();
    }

    private async Task ProcessAsync(ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        var envelope = JsonSerializer.Deserialize<EventEnvelope<TEvent>>(result.Message.Value, JsonDefaults.Options)
            ?? throw new InvalidOperationException($"Message on {Topic} is not a valid envelope");

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                await HandleAsync(scope.ServiceProvider, envelope, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < RetryDelays.Length && ex is not OperationCanceledException)
            {
                Logger.LogWarning(ex, "Handling {MessageId} from {Topic} failed (attempt {Attempt}), retrying", envelope.MessageId, Topic, attempt + 1);
                await Task.Delay(RetryDelays[attempt], cancellationToken);
            }
        }
    }

    private async Task DeadLetterAsync(ConsumeResult<string, string>? result, Exception exception, CancellationToken cancellationToken)
    {
        if (result is null)
        {
            return;
        }

        Logger.LogError(exception, "Dead-lettering message from {Topic} (key {Key})", Topic, result.Message.Key);

        var deadLetter = new Message<string, string>
        {
            Key = result.Message.Key,
            Value = result.Message.Value,
            Headers = new Headers
            {
                { "origin-topic", Encoding.UTF8.GetBytes(Topic) },
                { "consumer-group", Encoding.UTF8.GetBytes(GroupId) },
                { "error", Encoding.UTF8.GetBytes(exception.Message) },
            },
        };

        try
        {
            await producer.ProduceAsync(KafkaTopics.DeadLetter, deadLetter, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogError(ex, "Failed to dead-letter message from {Topic}; skipping it", Topic);
        }
    }
}
