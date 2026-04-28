using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json.Nodes;
using WJb;
using WJb.Actions;
using WJb.Extensions;

Console.OutputEncoding = Encoding.UTF8;

// ---------------------------------------------------------------------
// Action registry
// ---------------------------------------------------------------------

var actions = new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
{
    ["fib-start"] = ActionItemFactory.Create(
        typeof(FibonacciStartAction).AssemblyQualifiedName!,
        new JsonObject
        {
            ["next"] = "fib-build"
        }),

    ["fib-build"] = ActionItemFactory.Create(
        typeof(FibonacciBuildAction).AssemblyQualifiedName!,
        more: null)
};

// ---------------------------------------------------------------------
// Host setup
// ---------------------------------------------------------------------

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o => o.SingleLine = true);
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
    })
    .ConfigureServices(services =>
    {
        services.AddWJb(actions);
    })
    .Build();

// ---------------------------------------------------------------------
// Kick off initial job
// ---------------------------------------------------------------------

var jobs = host.Services.GetRequiredService<IJobProcessor>();

var job = await jobs.CompactAsync(
    "fib-start",
    new
    {
        from = 10,
        to = 100
    });

await jobs.EnqueueJobAsync(job);

// ---------------------------------------------------------------------

await host.RunAsync();

// =====================================================================
// Actions
// =====================================================================

public sealed class FibonacciStartAction(
    IJobProcessor jobs,
    ILogger<FibonacciStartAction> logger)
    : WorkflowActionBase
{
    private readonly IJobProcessor _jobs = jobs;
    private readonly ILogger<FibonacciStartAction> _logger = logger;

    protected override Task ExecCoreAsync(
        JsonObject? more,
        CancellationToken _)
    {
        ArgumentNullException.ThrowIfNull(more);

        var from = GetRequiredInt64(more, "from");
        var to = GetRequiredInt64(more, "to");

        long a = 0;
        long b = 1;

        while (a < from)
        {
            var next = a + b;
            a = b;
            b = next;
        }

        more["a"] = a;
        more["b"] = b;

        _logger.LogInformation(
            "Start Fibonacci range [{From}..{To}]: a={A}, b={B}",
            from, to, a, b);

        return Task.CompletedTask;
    }

    protected override async Task ExecNextAsync(
        bool success,
        JsonObject more,
        CancellationToken stoppingToken)
    {
        if (!success)
            return;

        var next = more.GetString("next");
        if (string.IsNullOrWhiteSpace(next))
            return;

        var job = await _jobs.CompactAsync(
            next,
            new
            {
                from = more.GetString("from"),
                to = more.GetString("to"),
                a = more.GetString("a"),
                b = more.GetString("b")
            },
            stoppingToken);

        await _jobs.EnqueueJobAsync(job, Priority.Normal, stoppingToken);
    }

    private static long GetRequiredInt64(JsonObject more, string name)
    {
        var s = more.GetString(name);
        if (!long.TryParse(s, out var value))
            throw new ArgumentException($"'{name}' must be a valid Int64.");

        return value;
    }
}

// ---------------------------------------------------------------------

public sealed class FibonacciBuildAction(
    ILogger<FibonacciBuildAction> logger)
    : IAction
{
    private readonly ILogger<FibonacciBuildAction> _logger = logger;

    public Task ExecAsync(
        JsonObject? more,
        CancellationToken _)
    {
        ArgumentNullException.ThrowIfNull(more);

        var from = GetRequiredInt64(more, "from");
        var to = GetRequiredInt64(more, "to");
        var a = GetRequiredInt64(more, "a");
        var b = GetRequiredInt64(more, "b");

        var values = new JsonArray();

        while (a <= to)
        {
            values.Add(a);
            var next = a + b;
            a = b;
            b = next;
        }

        _logger.LogInformation(
            "Fibonacci values [{From}..{To}] = {Result}",
            from,
            to,
            values.ToJsonString());

        return Task.CompletedTask;
    }

    private static long GetRequiredInt64(JsonObject more, string name)
    {
        var s = more.GetString(name);
        if (!long.TryParse(s, out var value))
            throw new ArgumentException($"'{name}' must be a valid Int64.");

        return value;
    }
}
