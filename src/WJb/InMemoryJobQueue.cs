using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace WJb;

/// <summary>
/// In-memory implementation of IJobQueue with priority support.
/// </summary>
public sealed class InMemoryJobQueue(ILogger<InMemoryJobQueue> logger) : IJobQueue
{
    private readonly ILogger<InMemoryJobQueue> _logger = logger;
    private readonly Channel<string> _asap = Channel.CreateUnbounded<string>();
    private readonly Channel<string> _high = Channel.CreateUnbounded<string>();
    private readonly Channel<string> _normal = Channel.CreateUnbounded<string>();
    private readonly Channel<string> _low = Channel.CreateUnbounded<string>();

    /// <summary>
    /// Enqueues a job with the specified priority.
    /// </summary>
    public async Task EnqueueAsync(string job, Priority priority, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Enqueue job '{Job}' with priority {Priority}", job, priority);

        Channel<string> channel = priority switch
        {
            Priority.ASAP => _asap,
            Priority.High => _high,
            Priority.Normal => _normal,
            Priority.Low => _low,
            _ => throw new ArgumentOutOfRangeException(nameof(priority))
        };

        await channel.Writer.WriteAsync(job, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Dequeues the next available job respecting priority order.
    /// </summary>
    public async Task<(string Job, Priority Priority)> DequeueNextAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        while (true)
        {
            if (_asap.Reader.TryRead(out var asap))
                return (asap, Priority.ASAP);

            if (_high.Reader.TryRead(out var high))
                return (high, Priority.High);

            if (_normal.Reader.TryRead(out var normal))
                return (normal, Priority.Normal);

            if (_low.Reader.TryRead(out var low))
                return (low, Priority.Low);

            var completed = await Task.WhenAny(
                _asap.Reader.WaitToReadAsync(cancellationToken).AsTask(),
                _high.Reader.WaitToReadAsync(cancellationToken).AsTask(),
                _normal.Reader.WaitToReadAsync(cancellationToken).AsTask(),
                _low.Reader.WaitToReadAsync(cancellationToken).AsTask()
            ).ConfigureAwait(false);

            // Observe cancellation explicitly
            await completed.ConfigureAwait(false);
        }
    }
}
