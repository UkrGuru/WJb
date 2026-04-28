using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

public sealed class JobProcessor_SmokeTests
{
    [Fact]
    public async Task Job_Is_Executed()
    {
        // Arrange
        TestAction.Executed = false;

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddWJb(new Dictionary<string, ActionItem>
                {
                    ["test"] = new ActionItem(
                        typeof(TestAction).AssemblyQualifiedName!,
                        null)
                });
            })
            .Build();

        await host.StartAsync();

        var jobs = host.Services.GetRequiredService<IJobProcessor>();

        // Act
        var job = await jobs.CompactAsync("test");
        await jobs.EnqueueJobAsync(job);

        // Give processor time to run
        await Task.Delay(200);

        // Assert
        Assert.True(TestAction.Executed);

        await host.StopAsync();
    }

    [Fact]
    public async Task Failed_Action_Does_Not_Stop_Processor()
    {
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(s =>
            {
                s.AddLogging();
                s.AddWJb(new Dictionary<string, ActionItem>
                {
                    ["fail"] = new ActionItem(typeof(FailingAction).AssemblyQualifiedName!, null),
                    ["ok"] = new ActionItem(typeof(OkAction).AssemblyQualifiedName!, null)
                });
            })
            .Build();

        await host.StartAsync();
        var jobs = host.Services.GetRequiredService<IJobProcessor>();

        await jobs.EnqueueJobAsync(await jobs.CompactAsync("fail"));
        await jobs.EnqueueJobAsync(await jobs.CompactAsync("ok"));

        await Task.Delay(300);

        Assert.True(OkAction.Executed);
    }

    sealed class FailingAction : IAction
    {
        public Task ExecAsync(JsonObject? _, CancellationToken __)
            => throw new InvalidOperationException("boom");
    }

    sealed class OkAction : IAction
    {
        public static bool Executed;
        public Task ExecAsync(JsonObject? _, CancellationToken __)
        {
            Executed = true;
            return Task.CompletedTask;
        }
    }

    private sealed class TestAction : IAction
    {
        public static bool Executed;

        public Task ExecAsync(JsonObject? _, CancellationToken __)
        {
            Executed = true;
            return Task.CompletedTask;
        }
    }
}
