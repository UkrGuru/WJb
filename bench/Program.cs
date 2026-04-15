using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WJb;
using WJb.Extensions;

namespace WJbBenchmark;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("WJb Performance Benchmark Suite v9");
        Console.WriteLine("==================================\n");

        var actions = new Dictionary<string, ActionItem>
        {
            ["benchmark"] = new ActionItem
            {
                Type = "WJbBenchmark.BenchmarkAction, WJbBenchmark",
                More = new JsonObject()
            }
        };

        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.None);   // Try to kill all noise
            })
            .ConfigureServices(services => services.AddWJb(actions))
            .Build();

        // Small delay to let WJb internal logging finish
        await host.StartAsync();
        await Task.Delay(100);

        var jobs = host.Services.GetRequiredService<IJobProcessor>();

        Console.WriteLine("WJb initialized.\n");

        await RunMultiThreadedEnqueue(jobs, 200_000, 8);
        await RunSingleThreadEnqueue(jobs, 100_000);
        await RunHeavyPayload(jobs, 20_000);
        await RunDelayedJobs(jobs, 10_000);

        Console.WriteLine("✅ Benchmark suite completed.");
        await host.StopAsync();
    }

    static async Task RunMultiThreadedEnqueue(IJobProcessor jobs, int totalJobs, int threadCount)
    {
        Console.WriteLine($"[Multi-threaded Enqueue] {totalJobs:N0} jobs from {threadCount} threads");
        var sw = Stopwatch.StartNew();

        var tasks = new Task[threadCount];
        int jobsPerThread = totalJobs / threadCount;

        for (int t = 0; t < threadCount; t++)
        {
            int threadId = t;
            tasks[t] = Task.Run(async () =>
            {
                for (int i = 0; i < jobsPerThread; i++)
                {
                    var job = await jobs.CompactAsync("benchmark", new { Thread = threadId, Id = i });
                    await jobs.EnqueueJobAsync(job);
                }
            });
        }

        await Task.WhenAll(tasks);
        sw.Stop();
        PrintResult(totalJobs, sw.Elapsed);
    }

    static async Task RunSingleThreadEnqueue(IJobProcessor jobs, int count)
    {
        Console.WriteLine($"[Single-thread Enqueue] {count:N0} jobs");
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < count; i++)
        {
            var job = await jobs.CompactAsync("benchmark", new { Id = i });
            await jobs.EnqueueJobAsync(job);
        }

        sw.Stop();
        PrintResult(count, sw.Elapsed);
    }

    static async Task RunHeavyPayload(IJobProcessor jobs, int count)
    {
        Console.WriteLine($"[Heavy JSON Payload] {count:N0} jobs");
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < count; i++)
        {
            var payload = new
            {
                Id = i,
                Time = DateTime.UtcNow,
                Data = Enumerable.Range(1, 100).ToArray(),
                Nested = new { Name = "Test", Values = new[] { 10, 20, 30 } }
            };

            var job = await jobs.CompactAsync("benchmark", payload);
            await jobs.EnqueueJobAsync(job);
        }

        sw.Stop();
        PrintResult(count, sw.Elapsed);
    }

    static async Task RunDelayedJobs(IJobProcessor jobs, int count)
    {
        Console.WriteLine($"[Delayed/Timer Jobs] {count:N0} jobs");
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < count; i++)
        {
            var job = await jobs.CompactAsync("benchmark", new { Id = i, Type = "delayed" });
            await jobs.EnqueueJobAsync(job);
        }

        sw.Stop();
        PrintResult(count, sw.Elapsed);
    }

    static void PrintResult(int count, TimeSpan elapsed)
    {
        var ms = elapsed.TotalMilliseconds;
        var throughput = count / (ms / 1000.0);
        Console.WriteLine($"    → {ms:F2} ms  |  {throughput:F0} jobs/sec\n");
    }
}

public sealed class BenchmarkAction : IAction
{
    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}