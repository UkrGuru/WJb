using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

var actions = new Dictionary<string, ActionItem>
{
    ["fib-start"] = new ActionItem
    {
        Type = "FibonacciStartAction, WorkflowWJb",
        More = new JsonObject { ["next"] = "fib-build" }
    },
    ["fib-build"] = new ActionItem
    {
        Type = "FibonacciBuildAction, WorkflowWJb"
    }
};

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(opt => { opt.SingleLine = true; });
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddWJb(actions);
    })
    .Build();

var jobs = host.Services.GetRequiredService<IJobProcessor>();

var job = await jobs.CompactAsync("fib-start", new { from = 4, to = 8 });

await jobs.EnqueueJobAsync(job);

await host.RunAsync();

// ------------------

public sealed class FibonacciStartAction(ILogger<FibonacciStartAction> logger) : IAction
{
    private readonly ILogger<FibonacciStartAction> _logger = logger;

    public Task ExecAsync(JsonObject? more, CancellationToken _)
    {
        ArgumentNullException.ThrowIfNull(more);

        var from = more.GetInt64("from") ?? throw new ArgumentNullException("from");
        var to = more.GetInt64("to") ?? throw new ArgumentNullException("to");

        long a = 0;
        long b = 1;

        for (int i = 0; i < from; i++)
        {
            var next = a + b;
            a = b;
            b = next;
        }

        more["next_from"] = from;
        more["next_to"] = to;
        more["next_a"] = a;
        more["next_b"] = b;

        logger.LogInformation("Start Fibonacci from {Index}: a={A}, b={B}", from, a, b);

        return Task.CompletedTask;
    }
}

public sealed class FibonacciBuildAction(ILogger<FibonacciStartAction> logger) : IAction
{
    private readonly ILogger<FibonacciStartAction> _logger = logger;

    public Task ExecAsync(JsonObject? more, CancellationToken _)
    {
        ArgumentNullException.ThrowIfNull(more);

        var from = more.GetInt64("from") ?? throw new ArgumentNullException("from");
        var to = more.GetInt64("to") ?? throw new ArgumentNullException("to");

        var a = more.GetInt64("a") ?? throw new ArgumentNullException("a");
        var b = more.GetInt64("b") ?? throw new ArgumentNullException("b");

        var values = new JsonArray();

        for (long i = from; i <= to; i++)
        {
            values.Add(a);

            var next = a + b;
            a = b;
            b = next;
        }

        logger.LogInformation("Fibonacci [{From}..{To}] = {Result}", from, to, values.ToJsonString());

        return Task.CompletedTask;
    }
}

