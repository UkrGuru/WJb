namespace WJb;

/// <summary>
/// Priority-aware job queue abstraction.
/// </summary>
public interface IJobQueue
{
    /// <summary>
    /// Enqueues a job with the specified priority.
    /// </summary>
    Task EnqueueAsync(string job, Priority priority, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues the next available job respecting priority order.
    /// </summary>
    Task<(string Job, Priority Priority)> DequeueNextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a processing slot for the given priority.
    /// </summary>
    void ReleaseSlot(Priority priority);

    /// <summary>
    /// Reloads queue configuration.
    /// </summary>
    Task ReloadAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}