# Priority WJb – Minimal Example

This example shows how to run **jobs with priorities** using **WJb**.

Jobs with higher priority are executed before lower‑priority ones.

***

## What happens

1.  The app starts a generic host
2.  A priority job queue is registered
3.  Three jobs are enqueued with different priorities
4.  The job processor executes them in priority order

Expected execution order:

1.  High
2.  Normal
3.  Low

***

## Register priority support

```csharp
services.AddSingleton<IJobQueue, SamplePriorityJobQueue>();

services.AddSingleton<SampleJobProcessor>();
services.AddSingleton<IJobProcessor>(
    sp => sp.GetRequiredService<SampleJobProcessor>());
services.AddHostedService(
    sp => sp.GetRequiredService<SampleJobProcessor>());
```

*   `SamplePriorityJobQueue` handles priority ordering
*   `SampleJobProcessor` processes jobs from the queue

***

## Enqueue jobs with priority

```csharp
await jobs.EnqueueJobAsync(
    await jobs.CompactAsync("print", new { text = "Low priority" }),
    Priority.Low);

await jobs.EnqueueJobAsync(
    await jobs.CompactAsync("print", new { text = "High priority" }),
    Priority.High);

await jobs.EnqueueJobAsync(
    await jobs.CompactAsync("print", new { text = "Normal priority" }),
    Priority.Normal);
```

*   Each job is enqueued with an explicit `Priority`
*   Higher priority jobs are processed first

***

## Action implementation

```csharp
public sealed class PrintAction(ILogger<PrintAction> logger) : IAction
{
    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
    {
        var text = jobMore?["text"]?.GetValue<string>() ?? "<empty>";
        logger.LogInformation(text);
        return Task.CompletedTask;
    }
}
```

***

## Run

```bash
dotnet run
```

Example output order:

```text
info: WJb.SampleJobProcessor[0]
      JobProcessor started
info: PrintAction[0]
      High priority
info: PrintAction[0]
      Normal priority
info: PrintAction[0]
      Low priority
```

***

## Summary

*   Jobs support priorities
*   Higher priority runs first
*   Same actions, same processor
*   Only the queue behavior changes

***
