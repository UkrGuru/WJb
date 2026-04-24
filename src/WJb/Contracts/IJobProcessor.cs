using System.Text.Json.Nodes;

namespace WJb;

/// <summary>
/// Processes jobs from a queue and executes actions.
/// </summary>
public interface IJobProcessor
{
    /// <summary>
    /// Compacts action code and metadata into a job payload.
    /// </summary>
    Task<string> CompactAsync(string actionCode, object? jobMore = null,
        CancellationToken stoppingToken = default);

    /// <summary>
    /// Enqueues a job for processing.
    /// </summary>
    Task EnqueueJobAsync(string job, Priority priority = Priority.Normal,
        CancellationToken stoppingToken = default);

    /// <summary>
    /// Expands a job payload into action code and metadata.
    /// </summary>
    Task<(string Code, JsonObject? More)> ExpandAsync(string job,
        CancellationToken stoppingToken = default);

    /// <summary>
    /// Processes a single job payload.
    /// </summary>
    Task ProcessJobAsync(string job, 
        CancellationToken stoppingToken = default);
}