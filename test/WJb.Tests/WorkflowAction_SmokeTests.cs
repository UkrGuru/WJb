using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Nodes;
using WJb;
using WJb.Actions;
using WJb.Extensions;

public sealed class WorkflowAction_SmokeTests
{
    [Fact]
    public async Task Core_And_Next_Are_Executed()
    {
        TestWorkflow.CoreCalled = false;
        TestWorkflow.NextCalled = false;

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddWJb(new Dictionary<string, ActionItem>
                {
                    ["wf"] = new ActionItem(
                        typeof(TestWorkflow).AssemblyQualifiedName!,
                        null)
                });
            })
            .Build();

        await host.StartAsync();

        var jobs = host.Services.GetRequiredService<IJobProcessor>();

        var job = await jobs.CompactAsync("wf");
        await jobs.EnqueueJobAsync(job);

        await Task.Delay(200);

        Assert.True(TestWorkflow.CoreCalled);
        Assert.True(TestWorkflow.NextCalled);

        await host.StopAsync();
    }

    private sealed class TestWorkflow : WorkflowActionBase
    {
        public static bool CoreCalled;
        public static bool NextCalled;

        protected override Task ExecCoreAsync(JsonObject? _, CancellationToken __)
        {
            CoreCalled = true;
            return Task.CompletedTask;
        }

        protected override Task ExecNextAsync(bool _, JsonObject __, CancellationToken ___)
        {
            NextCalled = true;
            return Task.CompletedTask;
        }
    }
}