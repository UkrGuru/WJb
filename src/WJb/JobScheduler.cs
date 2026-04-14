using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WJb.Extensions;
using WJb.Helpers;

namespace WJb;

public sealed class JobScheduler(
    IJobQueue jobQueue,
    IActionFactory factory, // ← single dependency (factory + reload interface)
    IJobProcessor jobProcessor,
    ILogger<JobScheduler> logger) : BackgroundService, IJobScheduler
{
    private readonly IJobQueue _jobQueue = jobQueue ?? throw new ArgumentNullException(nameof(jobQueue));
    private readonly IJobProcessor _jobProcessor = jobProcessor ?? throw new ArgumentNullException(nameof(jobProcessor));
    private readonly ILogger<JobScheduler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Dictionary<string, ActionItem> _cronActions = factory.Snapshot()
        .Where(kv => !string.IsNullOrWhiteSpace(kv.Value.More.GetString("cron")))
        .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase) ?? [];

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

                    var due = _cronActions.Where(kv => IsDue(kv.Value.More.GetString("cron"), now) == true).ToList() ?? [];

                    foreach (var (code, item) in due)
                    {
                        var job = await _jobProcessor.CompactAsync(code, null, stoppingToken);
                        await _jobQueue.EnqueueAsync(job, item.More.GetPriority(), stoppingToken);
                    }

                    await IntervalDelayAsync(DateTime.Now, stoppingToken);
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

    /// <inheritdoc/>
    public async Task IntervalDelayAsync(DateTime now, CancellationToken stoppingToken)
        => await Task.Delay(60000 - now.Second * 1000 - now.Millisecond + 25, stoppingToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public bool IsDue(string? cron, DateTime now) => CronHelper.CronValidate(cron, now);

    /// <inheritdoc/>
    public Task ReloadAsync(CancellationToken stoppingToken = default)
    {
        // Available only in the paid version.
        return Task.CompletedTask;
    }
}