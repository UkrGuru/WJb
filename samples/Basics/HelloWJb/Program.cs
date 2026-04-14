using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

var actions = new Dictionary<string, ActionItem>
{
    ["print"] = new ActionItem
    {
        Type = "PrintAction, HelloWJb",
        More = new JsonObject { ["name"] = "Oleksandr" }
    }
};

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddWJb(actions);
    })
    .Build();

var jobs = host.Services.GetRequiredService<IJobProcessor>();

var job = await jobs.CompactAsync("print", new { text = "Hello Viktor!" });

await jobs.EnqueueJobAsync(job);

await host.RunAsync();

// ------------------

public sealed class PrintAction(ILogger<PrintAction> logger) : IAction
{
    private readonly ILogger<PrintAction> _logger = logger;

    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
    {
        var text = jobMore?["text"]?.GetValue<string>() ?? "<empty>";
        _logger.LogInformation(text);
        return Task.CompletedTask;
    }
}
