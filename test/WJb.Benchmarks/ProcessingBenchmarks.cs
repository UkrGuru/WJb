using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

namespace WJb.Benchmarks;

/// <summary>
/// Processing throughput benchmarks for WJb runtime.
///
/// IMPORTANT:
/// - This benchmark measures ONLY runtime / processing cost.
/// - It must NEVER be compared to ingest-only benchmarks.
/// - Its purpose is relative comparison between runtime implementations.
///
/// Contract:
/// - JobProcessor is enabled
/// - Minimal action execution (Noop)
/// - Parallelism is controlled via MaxParallelJobs
/// </summary>
public static class ProcessingBenchmarks
{
    private static readonly int[] ParallelismLevels =
    {
        1,
        2,
        4,
        8,
        Environment.ProcessorCount
    };

    private const int JobsPerRun = 100_000;

    public static async Task RunAllAsync()
    {
        Console.WriteLine("WJb Processing Throughput Benchmarks");
        Console.WriteLine("====================================");

        foreach (var parallelism in ParallelismLevels)
        {
            await RunAsync(parallelism);
        }

        Console.WriteLine();
        Console.WriteLine("✅ All processing benchmarks completed");
    }

    private static async Task RunAsync(int maxParallelJobs)
    {
        Console.WriteLine();
        Console.WriteLine($"--- MaxParallelJobs = {maxParallelJobs} ---");

        // ------------------------------------------------------------------
        // Arrange
        // ------------------------------------------------------------------

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
            NullLogger<JobProcessor>.Instance,
            maxParallelJobs: maxParallelJobs);

        // ------------------------------------------------------------------
        // Preload jobs
        // ------------------------------------------------------------------

        for (int i = 0; i < JobsPerRun; i++)
        {
            var job = await processor.CompactAsync("noop", new { i });
            await processor.EnqueueJobAsync(job);
        }

        // ------------------------------------------------------------------
        // Start processing
        // ------------------------------------------------------------------

        using var cts = new CancellationTokenSource();

        var processingTask = Task.Run(() =>
            processor.RunAsync(cts.Token));

        // ------------------------------------------------------------------
        // Measure
        // ------------------------------------------------------------------

        var sw = Stopwatch.StartNew();

        while (queue.Count > 0)
        {
            await Task.Delay(1);
        }

        sw.Stop();

        cts.Cancel();
        await processingTask;

        var seconds = sw.Elapsed.TotalSeconds;
        var throughput = JobsPerRun / seconds;

        Console.WriteLine($"Processed jobs : {JobsPerRun:N0}");
        Console.WriteLine($"Time           : {seconds:F2} sec");
        Console.WriteLine($"Throughput     : {throughput:N0} jobs/sec");
    }

    // ------------------------------------------------------------------
    // Minimal action used for runtime measurement
    // ------------------------------------------------------------------

    private sealed class NoopAction : IAction
    {
        public Task ExecAsync(
            JsonObject? jobMore,
            CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}