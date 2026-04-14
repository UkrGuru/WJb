using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json.Nodes;

namespace WJb.Tests;

public sealed class JobProcessorTests
{
    /* =======================
       Compact / Expand
       ======================= */

    [Fact]
    public async Task Compact_And_Expand_Roundtrip()
    {
        var factory = new TestActionFactory();
        factory.Register("A", typeof(TestAction));

        var processor = CreateProcessor(factory);

        var job = await processor.CompactAsync(actionCode: "A", jobMore: new { X = 1 });

        var (type, more) = await processor.ExpandAsync(job);

        Assert.Equal(typeof(TestAction).AssemblyQualifiedName, type);
        Assert.Equal(1, more["X"]!.GetValue<int>());
    }

    /* =======================
       Enqueue
       ======================= */

    [Fact]
    public async Task EnqueueJob_Delegates_To_Queue()
    {
        var queue = new CapturingQueue();
        var processor = CreateProcessor(queue: queue);

        await processor.EnqueueJobAsync("job", Priority.High);

        Assert.Single(queue.Enqueued);
        Assert.Equal(("job", Priority.High), queue.Enqueued[0]);
    }

    /* =======================
       ProcessJobAsync
       ======================= */

    [Fact]
    public async Task ProcessJob_Executes_Action_And_Next_On_Success()
    {
        var action = new CapturingAction();
        var factory = new TestActionFactory();
        factory.Register("A", typeof(CapturingAction), action);

        var processor = CreateProcessor(factory);

        var job = await processor.CompactAsync("A", new { Value = 10, next = "A" });

        await processor.ProcessJobAsync(job, Priority.Normal);

        Assert.True(action.ExecCalled);
        Assert.True(action.NextCalled);
        Assert.True(action.NextMore!["__success"]!.GetValue<bool>());
        Assert.Equal("Normal", action.NextMore!["__priority"]!.GetValue<string>());
    }

    [Fact]
    public async Task ProcessJob_Sets_Success_False_When_Failure_Path_Is_Taken()
    {
        var action = new ThrowingAction();
        var factory = new TestActionFactory();
        factory.Register("A", typeof(ThrowingAction), action);

        var processor = CreateProcessor(factory);

        var job = await processor.CompactAsync("A", new { fail = "A" });

        await processor.ProcessJobAsync(job, Priority.Low);

        Assert.True(action.NextCalled);
        Assert.False(action.NextMore!["__success"]!.GetValue<bool>());
        Assert.Equal("Low", action.NextMore!["__priority"]!.GetValue<string>());
    }

    [Fact]
    public async Task ProcessJob_Cancellation_Skips_Action_Exec()
    {
        var action = new CancelAwareAction();
        var factory = new TestActionFactory();
        factory.Register("A", typeof(CancelAwareAction), action);

        var processor = CreateProcessor(factory);
        var job = await processor.CompactAsync("A");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await processor.ProcessJobAsync(job, Priority.Normal, cts.Token);

        Assert.False(action.ExecCalled);
    }


    internal sealed class CancelAwareAction : IAction
    {
        public bool ExecCalled;

        public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecCalled = true;
            return Task.CompletedTask;
        }
    }


    [Fact]
    public async Task ProcessJob_Executes_Next_On_Failure_When_Fail_Is_Defined()
    {
        var action = new ThrowingAction();
        var factory = new TestActionFactory();
        factory.Register("A", typeof(ThrowingAction), action);

        var processor = CreateProcessor(factory);

        var job = await processor.CompactAsync("A", new { fail = "A" });

        await processor.ProcessJobAsync(job, Priority.High);

        Assert.True(action.NextCalled);
    }

    /* =======================
       Helpers
       ======================= */

    private static JobProcessor CreateProcessor(
        TestActionFactory? factory = null,
        IJobQueue? queue = null)
    {
        return new JobProcessor(
            queue ?? new CapturingQueue(),
            factory ?? new TestActionFactory(),
            new NullLogger<JobProcessor>());
    }
}

/* =======================
   Test doubles
   ======================= */

internal sealed class CapturingQueue : IJobQueue
{
    public readonly List<(string Job, Priority Priority)> Enqueued = [];

    public Task EnqueueAsync(string job, Priority priority, CancellationToken cancellationToken = default)
    {
        Enqueued.Add((job, priority));
        return Task.CompletedTask;
    }

    public Task<(string Job, Priority Priority)> DequeueNextAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public void ReleaseSlot(Priority priority) { }
}

internal sealed class TestActionFactory : IActionFactory
{
    private readonly Dictionary<string, ActionItem> _items = new();
    private readonly Dictionary<Type, IAction> _instances = new();

    public event Action? Reloaded;

    public void Register(string code, Type actionType, IAction? instance = null)
    {
        _items[code] = new ActionItem(actionType.AssemblyQualifiedName!, null);

        if (instance != null)
            _instances[actionType] = instance;
    }

    public IAction Create(string actionType)
    {
        var type = Type.GetType(actionType)!;
        return _instances.TryGetValue(type, out var instance)
            ? instance
            : (IAction)Activator.CreateInstance(type)!;
    }

    public ActionItem GetActionItem(string actionCode)
        => _items[actionCode];

    public IReadOnlyDictionary<string, ActionItem> Snapshot()
    {
        throw new NotImplementedException();
    }

    public void Reload(IDictionary<string, ActionItem> newConfig)
    {
        throw new NotImplementedException();
    }
}

internal sealed class CapturingAction : IAction
{
    public bool ExecCalled;
    public bool NextCalled;
    public JsonObject? NextMore;

    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ExecCalled = true;
        return Task.CompletedTask;
    }


    public Task NextAsync(JsonObject nextMore, CancellationToken cancellationToken)
    {
        NextCalled = true;
        NextMore = nextMore;
        return Task.CompletedTask;
    }
}

internal sealed class ThrowingAction : IAction
{
    public bool NextCalled;
    public JsonObject? NextMore;

    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Boom");

    public Task NextAsync(JsonObject nextMore, CancellationToken cancellationToken)
    {
        NextCalled = true;
        NextMore = nextMore;
        return Task.CompletedTask;
    }
}