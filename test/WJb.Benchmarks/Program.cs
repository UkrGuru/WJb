
using WJb.Benchmarks;

await EnqueueBaselineBenchmarks.RunAsync();
await EnqueueBaselineMultithreadBenchmarks.RunAsync();
await QueueSemanticsBenchmarks.RunAllAsync();
await ProcessingBenchmarks.RunAllAsync();