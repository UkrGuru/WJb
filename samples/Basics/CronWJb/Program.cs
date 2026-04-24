using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

Console.OutputEncoding = Encoding.UTF8;

var actions = new Dictionary<string, ActionItem>
{
    ["HelloEveryMinute"] = new ActionItem
    {
        Type = "DummyAction, CronWJb",
        More = new JsonObject
        {
            ["cron"] = "* * * * *",
            ["priority"] = "ASAP",
            ["message"] = "Minute tick ✅"
        }
    },
    ["Hello9to5Weekdays"] = new ActionItem
    {
        Type = "DummyAction, CronWJb",
        More = new JsonObject
        {
            ["cron"] = "* 9-21 * * 1-5",
            ["priority"] = "High",
            ["message"] = "Working hours ping (every minute, Mon–Fri)"
        }
    }
};

var host = Host.CreateDefaultBuilder(args)
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
    .ConfigureServices((ctx, services) =>
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


// ✅ Action using ILogger
public sealed class DummyAction(ILogger<DummyAction> logger) : IAction
{
    private readonly ILogger<DummyAction> _logger = logger;

    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        var message = jobMore?.GetString("message") ?? "Hello from DummyAction!";

        _logger.LogInformation("{Message}", message);

        return Task.CompletedTask;
    }
}
