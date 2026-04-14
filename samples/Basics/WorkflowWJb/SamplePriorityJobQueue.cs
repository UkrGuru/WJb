using System.Collections.Concurrent;

namespace WJb;

public sealed class SamplePriorityJobQueue : IJobQueue
{
    private readonly ConcurrentQueue<string> _high = new();
    private readonly ConcurrentQueue<string> _normal = new();
    private readonly ConcurrentQueue<string> _low = new();

    private readonly SemaphoreSlim _signal = new(0);

    public Task EnqueueAsync(string job, Priority priority, CancellationToken _ = default)
    {
        switch (priority)
        {
            case Priority.High:
                _high.Enqueue(job);
                break;
            case Priority.Low:
                _low.Enqueue(job);
                break;
            default:
                _normal.Enqueue(job);
                break;
        }

        _signal.Release();
        return Task.CompletedTask;
    }

    public async Task<(string Job, Priority Priority)> DequeueNextAsync(
        CancellationToken cancellationToken = default)
    {
        await _signal.WaitAsync(cancellationToken);

        if (_high.TryDequeue(out var high))
            return (high, Priority.High);

        if (_normal.TryDequeue(out var normal))
            return (normal, Priority.Normal);

        if (_low.TryDequeue(out var low))
            return (low, Priority.Low);

        throw new InvalidOperationException("Queue signaled but no jobs available.");
    }

    public void ReleaseSlot(Priority _)
    {
        // no-op for samples
    }
}