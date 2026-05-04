using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json.Nodes;
using WJb;
using WJb.Actions;
using WJb.Extensions;

Console.OutputEncoding = Encoding.UTF8;

var actions = new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
{
    ["fib-start"] = new ActionItem(
        type: typeof(FibonacciStartAction).AssemblyQualifiedName!,
        more: new JsonObject
        {
            ["next"] = "fib-build"
        }),

    ["fib-build"] = new ActionItem(
        type: typeof(FibonacciBuildAction).AssemblyQualifiedName!,
        more: null)
};


using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o => o.SingleLine = true);
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
    })
    .ConfigureServices(services =>
    {
        services.AddWJb(actions);
    })
    .Build();

var jobs = host.Services.GetRequiredService<IJobProcessor>();

var job = await jobs.CompactAsync(
    "fib-start",
    new { from = 10, to = 100 });

await jobs.EnqueueJobAsync(job);

await host.RunAsync();
// ------------------


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

        var from = more?["from"]?.GetValue<long?>()
            ?? throw new ArgumentNullException("from");

        var to = more?["to"]?.GetValue<long?>()
            ?? throw new ArgumentNullException("to");

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
                from = more?["from"]?.GetValue<long?>(),
                to = more?["to"]?.GetValue<long?>(),
                a = more?["a"]?.GetValue<long?>(),
                b = more?["b"]?.GetValue<long?>()
            },
            stoppingToken);

        await _jobs.EnqueueJobAsync(job, Priority.Normal, stoppingToken);
    }
}

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

        var from = more?["from"]?.GetValue<long?>()
            ?? throw new ArgumentNullException("from");

        var to = more?["to"]?.GetValue<long?>()
            ?? throw new ArgumentNullException("to");

        var a = more?["a"]?.GetValue<long?>()
            ?? throw new ArgumentNullException("a");

        var b = more?["b"]?.GetValue<long?>()
            ?? throw new ArgumentNullException("b");

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
}