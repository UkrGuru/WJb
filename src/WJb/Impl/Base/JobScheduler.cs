// WJb – Base Edition
// Copyright (c) 2025–2026 Oleksandr Viktor (UkrGuru).
// Licensed under the WJb Base License.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WJb.Extensions;
using WJb.Helpers;

namespace WJb.Impl.Base;

/// <summary>
/// Background scheduler that enqueues jobs based on cron configuration.
/// Free edition implementation (snapshot-based).
/// </summary>
internal sealed class JobScheduler : BackgroundService, IJobScheduler
{
    private readonly IJobQueue _jobQueue;
    private readonly IJobProcessor _jobProcessor;
    private readonly ILogger<JobScheduler> _logger;

    // NOTE:
    // Cron actions are snapshotted once at construction time.
    // Changes to action metadata are not observed after startup.
    private readonly Dictionary<string, ActionItem> _cronActions;

    public JobScheduler(
        IJobQueue jobQueue,
        IActionFactory factory,
        IJobProcessor jobProcessor,
        ILogger<JobScheduler> logger)
    {
        _jobQueue = jobQueue ?? throw new ArgumentNullException(nameof(jobQueue));
        _jobProcessor = jobProcessor ?? throw new ArgumentNullException(nameof(jobProcessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        _cronActions = factory.Snapshot()
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value.More.GetString("cron")))
            .ToDictionary(
                kv => kv.Key,
                kv => kv.Value,
                StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public bool IsDue(string? cron, DateTime now)
        => CronHelper.CronValidate(cron, now);

    /// <inheritdoc/>
    public async Task ProcessDueCronActionsAsync(
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var (code, item) in _cronActions)
        {
            if (!IsDue(item.More.GetString("cron"), now))
                continue;

            var job = await _jobProcessor
                .CompactAsync(code, null, cancellationToken)
                .ConfigureAwait(false);

            await _jobQueue
                .EnqueueAsync(job, item.More.GetPriority(), cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// BackgroundService execution loop.
    /// Hosting and timing policy only; not part of the scheduling contract.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobScheduler started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;

                    await ProcessDueCronActionsAsync(now, stoppingToken)
                        .ConfigureAwait(false);

                    await DelayUntilNextMinuteAsync(now, stoppingToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    /* graceful shutdown */
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "JobScheduler crashed unexpectedly.");
                }
            }
        }
        finally
        {
            _logger.LogInformation("JobScheduler stopped");
        }
    }

    private static Task DelayUntilNextMinuteAsync(
        DateTime now,
        CancellationToken cancellationToken)
    {
        var delayMs = 60000 - now.Second * 1000 - now.Millisecond + 25;
        return Task.Delay(delayMs, cancellationToken);
    }
}
