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
        factory.Register("A", typeof(PTestAction));

        var processor = CreateProcessor(factory);

        var job = await processor.CompactAsync("A", new { X = 1 });

        var (code, more) = await processor.ExpandAsync(job);

        Assert.Equal("A", code);
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
    public async Task ProcessJob_Executes_Action_On_Success()
    {
        var action = new CapturingAction();
        var factory = new TestActionFactory();
        factory.Register("A", typeof(CapturingAction), action);

        var queue = new CapturingQueue();
        var processor = CreateProcessor(factory, queue);

        var job = await processor.CompactAsync("A", new { X = 1 });

        await processor.ProcessJobAsync(job);

        Assert.True(action.ExecCalled);
        Assert.Empty(queue.Enqueued);
    }

    [Fact]
    public async Task ProcessJob_Action_Exception_Is_Caught_And_Does_Not_Enqueue()
    {
        var action = new ThrowingAction();
        var factory = new TestActionFactory();
        factory.Register("A", typeof(ThrowingAction), action);

        var queue = new CapturingQueue();
        var processor = CreateProcessor(factory, queue);

        var job = await processor.CompactAsync("A");

        await processor.ProcessJobAsync(job);

        Assert.Empty(queue.Enqueued);
    }

    [Fact]
    public async Task ProcessJob_Cancellation_Skips_Action()
    {
        var action = new CapturingAction();
        var factory = new TestActionFactory();
        factory.Register("A", typeof(CapturingAction), action);

        var queue = new CapturingQueue();
        var processor = CreateProcessor(factory, queue);

        var job = await processor.CompactAsync("A");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await processor.ProcessJobAsync(job, cts.Token);

        Assert.False(action.ExecCalled);
        Assert.Empty(queue.Enqueued);
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

    public Task EnqueueAsync(
        string job,
        Priority priority,
        CancellationToken cancellationToken = default)
    {
        Enqueued.Add((job, priority));
        return Task.CompletedTask;
    }

    public Task<(string Job, Priority Priority)> DequeueNextAsync(
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public void ReleaseSlot(Priority priority) { }
}

internal sealed class TestActionFactory : IActionFactory
{
    private readonly Dictionary<string, ActionItem> _items =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<Type, IAction> _instances = new();

    public void Register(string code, Type actionType, IAction? instance = null)
    {
        _items[code] =
            new ActionItem(actionType.AssemblyQualifiedName!, null);

        if (instance != null)
            _instances[actionType] = instance;
    }

    public IAction Create(string actionCode)
    {
        var typeName = _items[actionCode].Type!;
        var type = Type.GetType(typeName)!;

        return _instances.TryGetValue(type, out var instance)
            ? instance
            : (IAction)Activator.CreateInstance(type)!;
    }

    public ActionItem GetActionItem(string actionCode)
        => _items[actionCode];

    public IReadOnlyDictionary<string, ActionItem> Snapshot()
        => _items;
}

internal sealed class CapturingAction : IAction
{
    public bool ExecCalled;

    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ExecCalled = true;
        return Task.CompletedTask;
    }
}

internal sealed class ThrowingAction : IAction
{
    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
        => throw new InvalidOperationException("Boom");
}

internal sealed class PTestAction : IAction
{
    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
        => Task.CompletedTask;
}