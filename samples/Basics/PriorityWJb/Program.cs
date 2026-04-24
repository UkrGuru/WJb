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
    ["print"] = new ActionItem
    {
        Type = "PrintAction, PriorityWJb",
        More = new JsonObject { ["name"] = "Oleksandr" }
    }
};

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(opt => { opt.SingleLine = true; });
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddWJb(actions);
    })
    .Build();

var jobs = host.Services.GetRequiredService<IJobProcessor>();

await jobs.EnqueueJobAsync(
    await jobs.CompactAsync("print", new { text = "Low priority" }),
    Priority.Low);

await jobs.EnqueueJobAsync(
    await jobs.CompactAsync("print", new { text = "High priority" }),
    Priority.High);

await jobs.EnqueueJobAsync(
    await jobs.CompactAsync("print", new { text = "Normal priority" }),
    Priority.Normal);

await host.RunAsync();


// ----------------------------

public sealed class PrintAction(ILogger<PrintAction> logger) : IAction
{
    private readonly ILogger<PrintAction> _logger = logger;

    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
    {
        var text = jobMore.GetString("text") ?? "<empty>";
        _logger.LogInformation(text);
        return Task.CompletedTask;
    }
}