using Microsoft.Extensions.Logging.Abstractions;
using WJb;

namespace WJb.Benchmarks;

/// <summary>
/// Queue semantics benchmarks for WJb core.
///
/// DESIGN CONTRACT (STRICT PRIORITY):
/// - Strict priority ordering (higher priority always wins)
/// - FIFO within the same priority
/// - Starvation of lower priorities is ALLOWED by design
/// - NO fairness guarantees
///
/// These are NOT performance benchmarks.
/// These are executable architectural contracts.
/// </summary>
public static class QueueSemanticsBenchmarks
{
    public static async Task RunAllAsync()
    {
        Console.WriteLine("WJb Queue Semantics Benchmarks (Strict Priority)");
        Console.WriteLine("================================================");

        await FIFO_Within_Same_Priority();
        await Higher_Priority_Takes_Precedence();

        Console.WriteLine();
        Console.WriteLine("✅ All queue semantics benchmarks PASSED");
    }

    // ------------------------------------------------------------------
    // FIFO within the same priority
    // ------------------------------------------------------------------

    /// <summary>
    /// Contract:
    /// Jobs with the same priority MUST be dequeued in strict FIFO order.
    /// </summary>
    private static async Task FIFO_Within_Same_Priority()
    {
        var queue = new InMemoryJobQueue(
            NullLogger<InMemoryJobQueue>.Instance);

        await queue.EnqueueAsync("A", Priority.Normal);
        await queue.EnqueueAsync("B", Priority.Normal);
        await queue.EnqueueAsync("C", Priority.Normal);

        var (j1, p1) = await queue.DequeueNextAsync(CancellationToken.None);
        queue.ReleaseSlot(p1);

        var (j2, p2) = await queue.DequeueNextAsync(CancellationToken.None);
        queue.ReleaseSlot(p2);

        var (j3, p3) = await queue.DequeueNextAsync(CancellationToken.None);
        queue.ReleaseSlot(p3);

        if (j1 != "A" || j2 != "B" || j3 != "C")
        {
            throw new Exception(
                $"FIFO violation within same priority: got [{j1}, {j2}, {j3}]");
        }

        Console.WriteLine("✔ FIFO within same priority");
    }

    // ------------------------------------------------------------------
    // Strict priority precedence
    // ------------------------------------------------------------------

    /// <summary>
    /// Contract:
    /// A higher-priority job MUST be dequeued before a lower-priority job
    /// if both are present in the queue.
    /// </summary>
    private static async Task Higher_Priority_Takes_Precedence()
    {
        var queue = new InMemoryJobQueue(
            NullLogger<InMemoryJobQueue>.Instance);

        await queue.EnqueueAsync("low-1", Priority.Low);
        await queue.EnqueueAsync("high-1", Priority.High);
        await queue.EnqueueAsync("low-2", Priority.Low);

        var (job, prio) = await queue.DequeueNextAsync(CancellationToken.None);
        queue.ReleaseSlot(prio);

        if (job != "high-1" || prio != Priority.High)
        {
            throw new Exception(
                $"Priority violation: expected [high-1, High], got [{job}, {prio}]");
        }

        Console.WriteLine("✔ Strict priority precedence");
    }
}