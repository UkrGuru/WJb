
using WJb.Benchmarks;

await EnqueueBaselineBenchmarks.RunAsync();
await EnqueueBaselineMultithreadBenchmarks.RunAsync();
await QueueSemanticsBenchmarks.RunAllAsync();

// ProcessingBenchmarks are deferred intentionally.
// await ProcessingBenchmarks.RunAllAsync();
