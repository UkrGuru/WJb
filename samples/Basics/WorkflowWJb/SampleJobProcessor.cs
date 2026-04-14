using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WJb;

public sealed class SampleJobProcessor(IJobQueue queue, IActionFactory factory, ILogger<SampleJobProcessor> logger) : BackgroundService, IJobProcessor
{
    private readonly IJobQueue _queue = queue;
    private readonly IActionFactory _factory = factory;
    private readonly ILogger<SampleJobProcessor> _logger = logger;

    public Task EnqueueJobAsync(string job, Priority priority = Priority.Normal, CancellationToken ct = default)
        => _queue.EnqueueAsync(job, priority, ct);

    public Task<string> CompactAsync(string actionCode, object? jobMore = null, CancellationToken ct = default)
    {
        var obj = new JsonObject
        {
            ["code"] = actionCode,
            ["more"] = JsonSerializer.SerializeToNode(jobMore)
        };
        return Task.FromResult(obj.ToJsonString());
    }

    public Task<(string Type, JsonObject More)> ExpandAsync(string job, CancellationToken ct = default)
    {
        var obj = JsonNode.Parse(job)!.AsObject();
        var code = obj["code"]!.GetValue<string>();
        var more = obj["more"]?.AsObject() ?? new JsonObject();
        var item = _factory.GetActionItem(code);
        return Task.FromResult((item.Type, more));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var (job, prio) = await _queue.DequeueNextAsync(stoppingToken);
            await ProcessJobAsync(job, prio, stoppingToken);
            _queue.ReleaseSlot(prio);
        }
    }

    public async Task ProcessJobAsync(string job, Priority priority = Priority.Normal, CancellationToken ct = default)
    {
        var (type, more) = await ExpandAsync(job, ct);
        var action = _factory.Create(type);

        // 1️⃣ Execute
        await action.ExecAsync(more, ct);

        // 2️⃣ IMPORTANT: allow action to route
        await action.NextAsync(more, ct);
    }
}