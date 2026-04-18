/*
 * WJb Multi-thread Enqueue Baseline Benchmark (Core / No DI)
 *
 * Purpose:
 *   Protect ingest scalability of WJb core.
 *
 * Contract:
 *   - Measures ONLY job compact + enqueue.
 *   - NO DI, NO hosting, NO workers.
 *   - Shared queue & processor across threads.
 *
 * Baseline:
 *   >= 250,000 jobs/sec on modern desktop hardware
 *   (Release, x64, ProcessorCount threads).
 */

using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

namespace WJb.Benchmarks;

public static class EnqueueBaselineMultithreadBenchmarks
{
    private const int WarmupJobsPerThread = 10_000;
    private const int TotalBenchmarkJobs = 1_000_000;

    public static async Task RunAsync()
    {
        var threads = Environment.ProcessorCount;
        Console.WriteLine("WJb Multi-thread Enqueue Baseline (Core, No DI)");
        Console.WriteLine("================================================");
        Console.WriteLine($"Threads   : {threads}");
        Console.WriteLine($"Total jobs: {TotalBenchmarkJobs:N0}");

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

        var warmupTasks = new Task[threads];
        for (int t = 0; t < threads; t++)
        {
            int tid = t;
            warmupTasks[t] = Task.Run(async () =>
            {
                for (int i = 0; i < WarmupJobsPerThread; i++)
                {
                    var job = await processor.CompactAsync(
                        "noop", new { t = tid, i });
                    await processor.EnqueueJobAsync(job);
                }
            });
        }

        await Task.WhenAll(warmupTasks);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // ---------- Benchmark ----------

        int jobsPerThread = TotalBenchmarkJobs / threads;

        var sw = Stopwatch.StartNew();

        var tasks = new Task[threads];
        for (int t = 0; t < threads; t++)
        {
            int tid = t;
            tasks[t] = Task.Run(async () =>
            {
                for (int i = 0; i < jobsPerThread; i++)
                {
                    var job = await processor.CompactAsync(
                        "noop", new { t = tid, i });
                    await processor.EnqueueJobAsync(job);
                }
            });
        }

        await Task.WhenAll(tasks);

        sw.Stop();

        var seconds = sw.Elapsed.TotalSeconds;
        var throughput = TotalBenchmarkJobs / seconds;

        Console.WriteLine();
        Console.WriteLine($"Time        : {seconds:F2} sec");
        Console.WriteLine($"Throughput  : {throughput:N0} jobs/sec");
        Console.WriteLine();

        // ---------- Baseline Guard ----------

        const int MinThroughput = 250_000;
        if (throughput < MinThroughput)
        {
            throw new Exception(
                $"MULTI-THREAD ENQUEUE REGRESSION: {throughput:N0} jobs/sec");
        }

        Console.WriteLine("✅ Multi-thread enqueue baseline PASSED");
    }

    private sealed class NoopAction : IAction
    {
        public Task ExecAsync(
            JsonObject? jobMore,
            CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}