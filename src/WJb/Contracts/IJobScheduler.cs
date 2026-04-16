namespace WJb;

/// <summary>
/// Schedules jobs based on time-based rules.
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Delays execution until the next scheduling interval.
    /// </summary>
    Task IntervalDelayAsync(DateTime now, CancellationToken stoppingToken);

    /// <summary>
    /// Determines whether a cron expression is due at the specified time.
    /// </summary>
    bool IsDue(string? cron, DateTime now);

    /// <summary>
    /// Reloads scheduler configuration.
    /// </summary>
    Task ReloadAsync(CancellationToken stoppingToken = default);
}
