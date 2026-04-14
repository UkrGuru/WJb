using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

var actions = new Dictionary<string, ActionItem>
{
    ["first"] = new ActionItem
    {
        Type = "FirstAction, WorkflowWJb",
        More = new JsonObject { ["next"] = "second" }
    },
    ["second"] = new ActionItem
    {
        Type = "SecondAction, WorkflowWJb"
    }
};


using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddWJb(actions);
    })
    .Build();

var jobs = host.Services.GetRequiredService<IJobProcessor>();

// Start the chain with the first action
var job = await jobs.CompactAsync("first");

await jobs.EnqueueJobAsync(job);

await host.RunAsync();

public sealed class FirstAction(ILogger<FirstAction> logger, IJobProcessor jobs) : IAction
{
    private readonly ILogger<FirstAction> _logger = logger;
    private readonly IJobProcessor _jobs = jobs;

    public Task ExecAsync(JsonObject? _, CancellationToken __)
    {
        _logger.LogInformation("First action executed");
        return Task.CompletedTask;
    }
}

public sealed class SecondAction(ILogger<SecondAction> logger) : IAction
{
    private readonly ILogger<SecondAction> _logger = logger;

    public Task ExecAsync(JsonObject? _, CancellationToken __)
    {
        _logger.LogInformation("Second action executed");
        return Task.CompletedTask;
    }
}