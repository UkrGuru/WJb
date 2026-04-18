using Microsoft.Extensions.Logging.Abstractions;

namespace WJb.Tests;

public sealed class InMemoryJobQueueTests
{
    [Fact]
    public async Task Enqueue_And_Dequeue_Single_Job()
    {
        var queue = CreateQueue();

        await queue.EnqueueAsync("job1", Priority.Normal);

        var (job, priority) = await queue.DequeueNextAsync();

        Assert.Equal("job1", job);
        Assert.Equal(Priority.Normal, priority);
    }

    [Fact]
    public async Task Dequeue_Respects_Priority_Order()
    {
        var queue = CreateQueue();

        await queue.EnqueueAsync("low", Priority.Low);
        await queue.EnqueueAsync("normal", Priority.Normal);
        await queue.EnqueueAsync("high", Priority.High);
        await queue.EnqueueAsync("asap", Priority.ASAP);

        Assert.Equal(("asap", Priority.ASAP), await queue.DequeueNextAsync());
        Assert.Equal(("high", Priority.High), await queue.DequeueNextAsync());
        Assert.Equal(("normal", Priority.Normal), await queue.DequeueNextAsync());
        Assert.Equal(("low", Priority.Low), await queue.DequeueNextAsync());
    }

    [Fact]
    public async Task Dequeue_Preserves_FIFO_Within_Same_Priority()
    {
        var queue = CreateQueue();

        await queue.EnqueueAsync("job1", Priority.High);
        await queue.EnqueueAsync("job2", Priority.High);
        await queue.EnqueueAsync("job3", Priority.High);

        Assert.Equal("job1", (await queue.DequeueNextAsync()).Job);
        Assert.Equal("job2", (await queue.DequeueNextAsync()).Job);
        Assert.Equal("job3", (await queue.DequeueNextAsync()).Job);
    }

    [Fact]
    public async Task Dequeue_Waits_Until_Job_Is_Available()
    {
        var queue = CreateQueue();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var dequeueTask = queue.DequeueNextAsync(cts.Token);

        await Task.Delay(100);

        await queue.EnqueueAsync("job", Priority.Normal);

        var result = await dequeueTask;

        Assert.Equal("job", result.Job);
        Assert.Equal(Priority.Normal, result.Priority);
    }

    [Fact]
    public async Task Dequeue_Throws_On_Cancellation()
    {
        var queue = CreateQueue();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            queue.DequeueNextAsync(cts.Token));
    }

    /* =======================
       Helpers
       ======================= */

    private static InMemoryJobQueue CreateQueue() 
        => new InMemoryJobQueue(new NullLogger<InMemoryJobQueue>());
}