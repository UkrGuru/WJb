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

    // ------------------------------- IJobProcessor -------------------------------

    /// <summary>
    /// Enqueues a job into the processing queue.
    /// </summary>
    public virtual async Task EnqueueJobAsync(string job, Priority priority = Priority.Normal, CancellationToken stoppingToken = default)
        => await _queue.EnqueueAsync(job, priority, stoppingToken).ConfigureAwait(false);

    /// <summary>
    /// Processes a single job with optional chaining.
    /// </summary>
    public async Task ProcessJobAsync(string job, Priority priority = Priority.Normal, CancellationToken stoppingToken = default)
    {
        bool success = true;
        string? actionType = null; JsonObject? mergedMore = null;

        try
        {
            // Expand job payload into:
            // - concrete action CLR type
            // - merged "more" object (action defaults + job overrides)
            var expanded = await ExpandAsync(job, stoppingToken).ConfigureAwait(false);
            actionType = expanded.Type; mergedMore = expanded.More;

            // Execute the action
            await JobProcessCoreAsync(actionType, mergedMore, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Cooperative cancellation: treat as non-successful execution
            _logger.LogInformation("ProcessJobAsync canceled. RawJob: {Job}", job);
            success = false;
        }
        catch (Exception ex)
        {
            // Action failure: log and mark execution as failed
            _logger.LogError(ex, "ProcessJobAsync crashed. RawJob: {Job}", job);
            success = false;
        }
        finally
        {
            // Preserve existing Next chaining semantics:
            // If the action defines a "next" step, enqueue it regardless of success,
            // letting the extractor decide based on the success flag.
            try
            {
                if (actionType is not null && mergedMore is not null)
                {
                    var nextMore = NextExtractor.ExtractNextMore(mergedMore, success);
                    if (nextMore is not null)
                    {
                        var nextCode = nextMore.GetString("__code")!;
                        var nextJob = await CompactAsync(nextCode, nextMore, stoppingToken).ConfigureAwait(false);

                        await EnqueueJobAsync(nextJob, priority, stoppingToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                // Errors in Next handling must not break the worker loop
                _logger.LogError(ex, "ProcessJobAsync Next crashed. RawJob: {Job}", job);
            }
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
    /// Expands a job payload into action type and merged metadata.
    /// </summary>
    public Task<(string Type, JsonObject More)> ExpandAsync(string job, CancellationToken stoppingToken = default)
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

        return Task.FromResult((item.Type, mergedMore));
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
            await ProcessJobAsync(job, prio, stoppingToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Executes a single action instance.
    /// </summary>
    protected virtual async Task JobProcessCoreAsync(string actionType, JsonObject mergedMore, CancellationToken stoppingToken)
    {
        var action = _factory.Create(actionType);
        await action.ExecAsync(mergedMore, stoppingToken).ConfigureAwait(false);
    }
}
