using System.Text.Json.Nodes;

namespace WJb;

public interface IJobProcessor
{
    Task EnqueueJobAsync(string job, Priority priority = Priority.Normal,
        CancellationToken stoppingToken = default);

    Task ProcessJobAsync(string job, Priority priority = Priority.Normal,
        CancellationToken stoppingToken = default);

    Task<string> CompactAsync(string actionCode, object? jobMore = null,
        CancellationToken stoppingToken = default);

    Task<(string Type, JsonObject More)> ExpandAsync(string job,
        CancellationToken stoppingToken = default);
}
