using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

Console.OutputEncoding = Encoding.UTF8;

// ---------------------------------------------------------------------
// Action registration
// ---------------------------------------------------------------------

var actions = new Dictionary<string, ActionItem>
{
    ["print"] = ActionItemFactory.Create(
        typeof(PrintAction).AssemblyQualifiedName!,
        new JsonObject
        {
            ["name"] = "Oleksandr"
        })
};

// ---------------------------------------------------------------------
// Host setup
// ---------------------------------------------------------------------

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(opt => opt.SingleLine = true);
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
    })
    .ConfigureServices(services =>
    {
        services.AddWJb(actions);
    })
    .Build();

// ---------------------------------------------------------------------
// Enqueue jobs with different priorities
// ---------------------------------------------------------------------

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

// ---------------------------------------------------------------------

await host.RunAsync();

// =====================================================================
// Action implementation
// =====================================================================

public sealed class PrintAction(ILogger<PrintAction> logger) : IAction
{
    private readonly ILogger<PrintAction> _logger = logger;

    public Task ExecAsync(
        JsonObject? jobMore,
        CancellationToken stoppingToken)
    {
        var text = jobMore.GetString("text") ?? "<empty>";
        _logger.LogInformation(text);
        return Task.CompletedTask;
    }
}