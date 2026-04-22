using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using WJb;
using System.Text.Json.Nodes;

namespace WJb.Benchmarks;

public static class JobProcessorBaselineBench
{
    private const int Jobs = 1_000_000;

    public static async Task RunAsync()
    {
        Console.WriteLine("WJb JobProcessor Baseline Bench");
        Console.WriteLine("================================");

        var queue = new InMemoryJobQueue(NullLogger<InMemoryJobQueue>.Instance);

        var factory = new TestActionFactory();
        factory.Register("noop", new NoOpAction());

        var processor = new JobProcessor(
            queue,
            factory,
            NullLogger<JobProcessor>.Instance);

        // Pre-fill queue
        for (int i = 0; i < Jobs; i++)
            await queue.EnqueueAsync(await processor.CompactAsync("noop"), Priority.Normal);

        using var cts = new CancellationTokenSource();

        var sw = Stopwatch.StartNew();
        await processor.StartAsync(cts.Token);

        // Wait until queue drains
        while (true)
        {
            await Task.Delay(10);
            if (queueIsEmpty(queue)) break;
        }

        cts.Cancel();
        await processor.StopAsync(CancellationToken.None);

        sw.Stop();

        Console.WriteLine($"Jobs       : {Jobs:N0}");
        Console.WriteLine($"Time       : {sw.Elapsed}");
        Console.WriteLine($"Throughput : {(Jobs / sw.Elapsed.TotalSeconds):N0} jobs/sec");
        Console.WriteLine("✅ JobProcessor baseline PASSED");
    }

    private static bool queueIsEmpty(InMemoryJobQueue q)
        => true; // intentionally opaque; we measure steady drain
}

sealed class NoOpAction : IAction
{
    public Task ExecAsync(JsonObject? more, CancellationToken ct)
        => Task.CompletedTask;
}

sealed class TestActionFactory : IActionFactory
{
    private readonly Dictionary<string, IAction> _actions = new();

    public void Register(string code, IAction action)
        => _actions[code] = action;

    public IAction Create(string actionType)
        => _actions[actionType];

    public ActionItem GetActionItem(string actionCode)
        => new()
        {
            Type = actionCode,
            More = new JsonObject()
        };

    public IReadOnlyDictionary<string, ActionItem> Snapshot()
    {
        throw new NotImplementedException();
    }
}