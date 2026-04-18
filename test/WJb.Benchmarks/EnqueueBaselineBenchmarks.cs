/*
 * WJb Enqueue Baseline Benchmark (Core / No DI)
 *
 * Purpose:
 *   Protect core ingest performance of WJb.
 *
 * Contract:
 *   - Measures ONLY job compact + enqueue.
 *   - NO DI, NO hosting, NO background workers.
 *   - Core components only.
 *
 * Baseline:
 *   ~600,000 jobs/sec on modern hardware.
 *
 * Policy:
 *   Any regression here must be justified
 *   by measurable new semantics.
 */

using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

namespace WJb.Benchmarks;

public static class EnqueueBaselineBenchmarks
{
    private const int WarmupJobs = 50_000;
    private const int BenchmarkJobs = 1_000_000;

    public static async Task RunAsync()
    {
        Console.WriteLine("WJb Enqueue Baseline Benchmark (Core, No DI)");
        Console.WriteLine("===========================================");

        // ---------- Arrange (CORE ONLY) ----------

        var actions = new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
        {
            ["noop"] = ActionItemFactory.Create(
                typeof(NoopAction).AssemblyQualifiedName!,
                new { })
        };

        var actionFactory = new ActionFactory(
            services: null,
            actions: actions);

        var queue = new InMemoryJobQueue(
            NullLogger<InMemoryJobQueue>.Instance);

        var processor = new JobProcessor(
            queue,
            actionFactory,
            NullLogger<JobProcessor>.Instance);
        // ---------- Warmup ----------

        for (int i = 0; i < WarmupJobs; i++)
        {
            var job = await processor.CompactAsync("noop", new { i });
            await processor.EnqueueJobAsync(job);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // ---------- Benchmark ----------

        var sw = Stopwatch.StartNew();

        for (int i = 0; i < BenchmarkJobs; i++)
        {
            var job = await processor.CompactAsync("noop", new { i });
            await processor.EnqueueJobAsync(job);
        }

        sw.Stop();

        var seconds = sw.Elapsed.TotalSeconds;
        var throughput = BenchmarkJobs / seconds;

        Console.WriteLine();
        Console.WriteLine($"Jobs       : {BenchmarkJobs:N0}");
        Console.WriteLine($"Time       : {seconds:F2} sec");
        Console.WriteLine($"Throughput : {throughput:N0} jobs/sec");
        Console.WriteLine();

        if (throughput < 500_000)
        {
            throw new Exception(
                $"ENQUEUE BASELINE REGRESSION: {throughput:N0} jobs/sec");
        }

        Console.WriteLine("✅ Enqueue baseline PASSED");
    }

    private sealed class NoopAction : IAction
    {
        public Task ExecAsync(
            JsonObject? jobMore,
            CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}