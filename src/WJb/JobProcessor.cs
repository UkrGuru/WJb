using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb.Extensions;

namespace WJb;

public class JobProcessor(IJobQueue queue, IActionFactory actionFactory, ILogger<JobProcessor> logger) : BackgroundService, IJobProcessor
{
    private readonly IJobQueue _queue = queue ?? throw new ArgumentNullException(nameof(queue));
    private readonly IActionFactory _factory = actionFactory ?? throw new ArgumentNullException(nameof(actionFactory));
    private readonly ILogger<JobProcessor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // ------------------------------- IJobProcessor -------------------------------
    public virtual async Task EnqueueJobAsync(string job, Priority priority = Priority.Normal, CancellationToken stoppingToken = default)
        => await _queue.EnqueueAsync(job, priority, stoppingToken).ConfigureAwait(false);

    public async Task ProcessJobAsync(string job, Priority priority = Priority.Normal, CancellationToken stoppingToken = default)
    {
        bool success = true;
        string? actionType = null; JsonObject? mergedMore = null;

        try
        {
            var expanded = await ExpandAsync(job, stoppingToken).ConfigureAwait(false);
            actionType = expanded.Type; mergedMore = expanded.More;

            await JobProcessCoreAsync(actionType, mergedMore, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("ProcessJobAsync canceled. RawJob: {Job}", job);
            success = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProcessJobAsync crashed. RawJob: {Job}", job);
            success = false;
        }
        finally
        {
            // Preserve existing Next chaining semantics
            try
            {
                if (actionType is not null && mergedMore is not null)
                {
                    var nextMore = NextExtractor.ExtractNextMore(mergedMore, success);
                    if (nextMore is not null)
                    {
                        nextMore["__success"] = success;
                        nextMore["__priority"] = priority.ToString();

                        var action = _factory.Create(actionType);
                        await action.NextAsync(nextMore, stoppingToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessJobAsync Next crashed. RawJob: {Job}", job);
            }
        }
    }

    public Task<string> CompactAsync(string actionCode, object? jobMore = null, CancellationToken stoppingToken = default)
    {
        var obj = new JsonObject
        {
            ["code"] = actionCode,
            ["more"] = MoreExtensions.ToJsonObject(jobMore)
        };
        return Task.FromResult(obj.ToJsonString());
    }

    public Task<(string Type, JsonObject More)> ExpandAsync(string job, CancellationToken stoppingToken = default)
    {

        var node = JsonNode.Parse(job)!.AsObject();
        var code = node.GetString("code")!;
        var more = node.GetObject("more") ?? [];

        var item = _factory.GetActionItem(code);
        return Task.FromResult((item.Type, more));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var (job, prio) = await _queue.DequeueNextAsync(stoppingToken).ConfigureAwait(false);
            _ = ProcessJobAsync(job, prio, stoppingToken);
        }
    }

    protected virtual async Task JobProcessCoreAsync(string actionType, JsonObject mergedMore, CancellationToken stoppingToken)
    {
        var action = _factory.Create(actionType);
        await action.ExecAsync(mergedMore, stoppingToken).ConfigureAwait(false);
    }
}