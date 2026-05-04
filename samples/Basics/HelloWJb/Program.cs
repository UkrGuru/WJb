using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

Console.OutputEncoding = Encoding.UTF8;

// --------------------------------------------------
// Actions
// --------------------------------------------------

var actions = new Dictionary<string, ActionItem>
{
    ["print"] = ActionItemFactory.Create(
        type: typeof(PrintAction).AssemblyQualifiedName!, more: new { name = "World!" })
};

// --------------------------------------------------
// Host
// --------------------------------------------------

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

// --------------------------------------------------
// Enqueue job
// --------------------------------------------------

var jobs = host.Services.GetRequiredService<IJobProcessor>();

var job = await jobs.CompactAsync("print", new { text = "Hello WJb!" });

await jobs.EnqueueJobAsync(job);

await host.RunAsync();

// --------------------------------------------------
// Action
// --------------------------------------------------

public sealed class PrintAction(ILogger<PrintAction> logger) : IAction
{
    private readonly ILogger<PrintAction> _logger = logger;

    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
    {
        var text = jobMore?.GetString("text") ?? "<empty>";
        _logger.LogInformation(text);
        return Task.CompletedTask;
    }
}