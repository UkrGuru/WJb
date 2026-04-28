using System.Text;
using WJb.Benchmarks;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("WJb JobProcessor – console benchmarks");
Console.WriteLine(".NET " + Environment.Version);
Console.WriteLine();

await JobProcessorBaselineBench.RunAsync();
await EnqueueBaselineBenchmarks.RunAsync();
await EnqueueBaselineMultithreadBenchmarks.RunAsync();
await QueueSemanticsBenchmarks.RunAllAsync();

// ProcessingBenchmarks are deferred intentionally.
// await ProcessingBenchmarks.RunAllAsync();


//WJb Enqueue Baseline Benchmark (Core, No DI)
//===========================================

//Jobs       : 1,000,000
//Time       : 1.42 sec
//Throughput : 705,098 jobs/sec

//✅ Enqueue baseline PASSED
//WJb Multi-thread Enqueue Baseline (Core, No DI)
//================================================
//Threads   : 20
//Total jobs: 1,000,000

//Time        : 0.50 sec
//Throughput  : 1,995,682 jobs/sec

//✅ Multi-thread enqueue baseline PASSED
//WJb Queue Semantics Benchmarks (Strict Priority)
//================================================
//✔ FIFO within same priority
//✔ Strict priority precedence

//✅ All queue semantics benchmarks PASSED