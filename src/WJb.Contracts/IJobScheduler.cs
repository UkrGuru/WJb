namespace WJb;

public interface IJobScheduler
{
    Task IntervalDelayAsync(DateTime now, CancellationToken stoppingToken);
    bool IsDue(string? cron, DateTime now);
    Task ReloadAsync(CancellationToken stoppingToken = default);
}
