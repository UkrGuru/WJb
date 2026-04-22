using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb.Extensions;

namespace WJb;

/// <summary>
/// Background job processor that executes queued jobs.
/// </summary>
/// 
public class JobProcessor(IJobQueue queue, IActionFactory actionFactory, ILogger<JobProcessor> logger, ISettingsRegistry? settings = default) : BackgroundService, IJobProcessor
{
    private readonly IJobQueue _queue = queue ?? throw new ArgumentNullException(nameof(queue));
    private readonly IActionFactory _factory = actionFactory ?? throw new ArgumentNullException(nameof(actionFactory));
    private readonly ILogger<JobProcessor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // Settings snapshot:
    // - Read once at construction time
    // - Application restart required to apply changes
    // - Hot reload is NOT supported in FREE edition
    private readonly ISettingsRegistry _settings = settings ?? SettingsRegistry.Empty;

    // NOTE:
    // Settings are read once at construction time.
    // Changes require application restart.
    // Hot reload is NOT supported in FREE edition.
    // Any runtime parallelism is fixed at startup.

    // ------------------------------- IJobProcessor -------------------------------

    /// <summary>
    /// Enqueues a job into the processing queue.
    /// </summary>
    public virtual async Task EnqueueJobAsync(string job, Priority priority = Priority.Normal, CancellationToken stoppingToken = default)
        => await _queue.EnqueueAsync(job, priority, stoppingToken).ConfigureAwait(false);

    /// <summary>
    /// Processes a single job by executing the resolved action.
    /// </summary>
    public async Task ProcessJobAsync(string job, CancellationToken stoppingToken = default)
    {
        // Design principle:
        // Simplicity → Explicitness → Extensibility
        //
        // Invariant:
        // - One job = one action execution
        // - No implicit chaining or routing
        //
        // Responsibility boundary:
        // - JobProcessor: execution only
        // - Workflow: explicit via WorkflowAction

        try
        {
            // - action code
            // - merged metadata (action defaults + job override)
            var expanded = await ExpandAsync(job, stoppingToken).ConfigureAwait(false);

            var actionCode = expanded.Code; 
            var execMore = expanded.More?.DeepClone() as JsonObject ?? [];

            // Execute the action
            var action = _factory.Create(actionCode);
            await action.ExecAsync(execMore, stoppingToken).ConfigureAwait(false);

            _logger.LogInformation("ProcessJobAsync done. RawJob: {Job}", job);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Cooperative cancellation: treat as non-successful execution
            _logger.LogInformation("ProcessJobAsync canceled. RawJob: {Job}", job);
        }
        catch (Exception ex)
        {
            // Action failure: log and mark execution as failed
            _logger.LogError(ex, "ProcessJobAsync crashed. RawJob: {Job}", job);
        }
    }

    /// <summary>
    /// Compacts an action code and metadata into a job payload.
    /// </summary>
    public Task<string> CompactAsync(string actionCode, object? jobMore = null, CancellationToken stoppingToken = default)
    {
        var obj = new JsonObject
        {
            ["code"] = actionCode,
            ["more"] = MoreExtensions.ToJsonObject(jobMore)
        };

        return Task.FromResult(obj.ToJsonString());
    }

    /// <summary>
    /// Expands a job payload into action code and merged metadata.
    /// </summary>
    public Task<(string Code, JsonObject More)> ExpandAsync(string job, CancellationToken stoppingToken = default)
    {
        // Parse raw job JSON
        var node = JsonNode.Parse(job)!.AsObject();

        // Extract logical action code and job-specific metadata
        var code = node.GetString("code")!;
        var more = node.GetObject("more") ?? [];

        // Resolve action definition from the factory
        var item = _factory.GetActionItem(code);

        // Clone action-level defaults to avoid mutating shared state
        var mergedMore = (item.More?.DeepClone() as JsonObject) ?? [];

        // Overlay job-level metadata on top of action defaults
        // (job values win)
        MoreExtensions.MergeInto(mergedMore, more);

        return Task.FromResult((Code: code, More: mergedMore));
    }

    /// <summary>
    /// Executes the processor loop.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Block until the next job is available,
            // always respecting priority ordering
            var (job, prio) = await _queue.DequeueNextAsync(stoppingToken).ConfigureAwait(false);

            // Process job end-to-end, including possible chaining
            await ProcessJobAsync(job, stoppingToken).ConfigureAwait(false);
        }
    }
}