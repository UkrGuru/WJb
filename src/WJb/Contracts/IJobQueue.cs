namespace WJb;

public interface IJobQueue
{
    Task EnqueueAsync(string job, Priority priority, CancellationToken cancellationToken = default);

    Task<(string Job, Priority Priority)> DequeueNextAsync(CancellationToken cancellationToken = default);

    void ReleaseSlot(Priority priority);

    Task ReloadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}