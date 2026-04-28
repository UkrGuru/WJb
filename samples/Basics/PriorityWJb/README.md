# PriorityWJb

A minimal example demonstrating
how to enqueue and execute jobs with **priorities**
using **WJb (Free edition)**.

---

## What this example demonstrates

1. A .NET host is started
2. A single action (`PrintAction`) is registered
3. Multiple jobs are enqueued explicitly
4. Each job is assigned a priority
5. The job processor processes jobs by priority order

There is no implicit execution or orchestration.

---

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
*   Higher priority jobs are processed before lower-priority ones
*   Priority affects queue ordering only

***

## Action implementation

```csharp
public sealed class PrintAction(ILogger<PrintAction> logger) : IAction
{
    public Task ExecAsync(JsonObject? jobMore, CancellationToken _)
    {
        var text = jobMore.GetString("text") ?? "<empty>";
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
info: WJb.JobProcessor[0] JobProcessor started
info: PrintAction[0] High priority
info: PrintAction[0] Normal priority
info: PrintAction[0] Low priority
```

***

## Summary

*   Jobs support explicit priorities
*   Higher priority jobs are executed first
*   The same action and processor are used
*   Only queue ordering behavior changes

***
