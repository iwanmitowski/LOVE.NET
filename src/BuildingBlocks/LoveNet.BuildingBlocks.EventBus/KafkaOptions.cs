namespace LoveNet.BuildingBlocks.EventBus;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:29092";
}
