using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

Console.OutputEncoding = Encoding.UTF8;

// ---------------------------------------------------------------------
// Action registration with cron metadata
// ---------------------------------------------------------------------

var actions = new Dictionary<string, ActionItem>
{
    ["HelloEveryMinute"] = ActionItemFactory.Create(
        typeof(DummyAction).AssemblyQualifiedName!,
        new JsonObject
        {
            ["cron"] = "* * * * *",
            ["priority"] = "ASAP",
            ["message"] = "Minute tick ✅"
        }),

    ["HelloWorkingHours"] = ActionItemFactory.Create(
        typeof(DummyAction).AssemblyQualifiedName!,
        new JsonObject
        {
            ["cron"] = "* 9-21 * * 1-5",
            ["priority"] = "High",
            ["message"] = "Working hours ping (Mon–Fri)"
        })
};

// ---------------------------------------------------------------------
// Host setup
// ---------------------------------------------------------------------

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(opt =>
        {
            opt.SingleLine = true;
            opt.TimestampFormat = "HH:mm:ss ";
        });

        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
    })
    .ConfigureServices(services =>
    {
        services.AddWJb(actions, addScheduler: true);
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("CronWJb started. Waiting for cron ticks...");

foreach (var kv in actions)
{
    var cron = kv.Value.More?.GetString("cron") ?? "(none)";
    logger.LogInformation(" - {Action}: {Cron}", kv.Key, cron);
}

await host.RunAsync();

// =====================================================================
// Action implementation
// =====================================================================

public sealed class DummyAction(ILogger<DummyAction> logger) : IAction
{
    private readonly ILogger<DummyAction> _logger = logger;

    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        var message = jobMore.GetString("message") ?? "Hello from DummyAction!";
        _logger.LogInformation("{Message}", message);
        return Task.CompletedTask;
    }
}