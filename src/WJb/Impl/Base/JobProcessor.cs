// WJb – Base Edition
// Copyright (c) 2025–2026 Oleksandr Viktor (UkrGuru).
// Licensed under the WJb Base License.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb.Extensions;

namespace WJb.Impl.Base;

/// <summary>
/// Base (FREE edition) job processor.
/// - No hot reload
/// - Settings read once at startup
/// - Fixed runtime behavior
/// </summary>
internal sealed class JobProcessor(
    IJobQueue queue,
    IActionFactory actionFactory,
    ILogger<JobProcessor> logger,
    ISettingsRegistry? settings = null)
    : BackgroundService, IJobProcessor
{
    private readonly IJobQueue _queue =
        queue ?? throw new ArgumentNullException(nameof(queue));

    private readonly IActionFactory _factory =
        actionFactory ?? throw new ArgumentNullException(nameof(actionFactory));

    private readonly ILogger<JobProcessor> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    // Settings snapshot:
    // - Read once at construction time
    // - Application restart required to apply changes
    // - Hot reload is NOT supported in Base edition
    private readonly ISettingsRegistry _settings =
        settings ?? SettingsRegistry.Empty;

    // ------------------------------- IJobProcessor -------------------------------

    /// <summary>
    /// Compacts an action code and metadata into a job payload.
    /// </summary>
    public Task<string> CompactAsync(
        string actionCode,
        object? jobMore = null,
        CancellationToken stoppingToken = default)
    {
        var obj = new JsonObject
        {
            ["code"] = actionCode,
            ["more"] = MoreExtensions.ToJsonObject(jobMore)
        };

        return Task.FromResult(obj.ToJsonString());
    }

    /// <summary>
    /// Enqueues a job into the processing queue.
    /// </summary>
    public Task EnqueueJobAsync(
        string job,
        Priority priority = Priority.Normal,
        CancellationToken stoppingToken = default)
        => _queue.EnqueueAsync(job, priority, stoppingToken);

    /// <summary>
    /// Expands a job payload into action code and merged metadata.
    /// </summary>
    public Task<(string Code, JsonObject More)> ExpandAsync(
        string job,
        CancellationToken stoppingToken = default)
    {
        var node = JsonNode.Parse(job)!.AsObject();

        var code = node.GetString("code")!;
        var more = node.GetObject("more") ?? [];

        var item = _factory.GetActionItem(code);

        // Clone defaults to avoid mutating shared state
        var mergedMore =
            (item.More?.DeepClone() as JsonObject) ?? [];

        // Job-level values win
        MoreExtensions.MergeInto(mergedMore, more);

        return Task.FromResult((code, mergedMore));
    }

    /// <summary>
    /// Processes a single job by executing the resolved action.
    /// </summary>
    public async Task ProcessJobAsync(
        string job,
        CancellationToken stoppingToken = default)
    {
        try
        {
            var (code, more) =
                await ExpandAsync(job, stoppingToken)
                    .ConfigureAwait(false);

            var execMore =
                more.DeepClone() as JsonObject ?? [];

            var action = _factory.Create(code);
            await action.ExecAsync(execMore, stoppingToken)
                .ConfigureAwait(false);

            _logger.LogDebug(
                "ProcessJobAsync done. RawJob: {Job}", job);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "ProcessJobAsync canceled. RawJob: {Job}", job);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "ProcessJobAsync crashed. RawJob: {Job}", job);
        }
    }

    // ------------------------------- BackgroundService -------------------------------

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Base JobProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var job =
                await _queue.DequeueNextAsync(stoppingToken)
                    .ConfigureAwait(false);

            await ProcessJobAsync(job, stoppingToken)
                .ConfigureAwait(false);
        }
    }
}