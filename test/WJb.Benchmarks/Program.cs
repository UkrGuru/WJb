using System.Text;
using WJb.Benchmarks;

Console.OutputEncoding = Encoding.UTF8;

await EnqueueBaselineBenchmarks.RunAsync();
await EnqueueBaselineMultithreadBenchmarks.RunAsync();
await QueueSemanticsBenchmarks.RunAllAsync();

// ProcessingBenchmarks are deferred intentionally.
// await ProcessingBenchmarks.RunAllAsync();
