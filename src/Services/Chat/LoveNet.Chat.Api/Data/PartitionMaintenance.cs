using Microsoft.EntityFrameworkCore;

namespace LoveNet.Chat.Api.Data;

/// <summary>
/// The messages table is range-partitioned by created_on. A DEFAULT partition
/// (created in the initial migration) catches anything unexpected; this
/// routine pre-creates explicit monthly partitions around "now" at startup.
/// </summary>
public static class PartitionMaintenance
{
    public static async Task EnsureMonthlyMessagePartitionsAsync(ChatDbContext db, int monthsBehind = 1, int monthsAhead = 3)
    {
        if (!db.Database.IsRelational() || db.Database.ProviderName?.Contains("Npgsql") != true)
        {
            return;
        }

        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-monthsBehind);

        for (var i = 0; i <= monthsBehind + monthsAhead; i++)
        {
            var from = start.AddMonths(i);
            var to = from.AddMonths(1);
            var name = $"messages_y{from:yyyy}m{from:MM}";

            await db.Database.ExecuteSqlRawAsync(
                $"""
                CREATE TABLE IF NOT EXISTS {name}
                PARTITION OF messages
                FOR VALUES FROM ('{from:yyyy-MM-dd}') TO ('{to:yyyy-MM-dd}');
                """);
        }
    }
}
