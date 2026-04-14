# Workflow WJb – Minimal Example

This example shows how to build a **job workflow** in **WJb**, where one action **automatically schedules the next action**.

***

## What happens

1.  The app starts a generic host
2.  The first job (`first`) is enqueued
3.  `FirstAction` runs
4.  `FirstAction` enqueues the next job (`second`)
5.  `SecondAction` runs

Execution order is strictly controlled by the workflow.

***

## Register actions and processor

```csharp
services.AddSingleton<FirstAction>();
services.AddSingleton<SecondAction>();

services.AddSingleton<IActionFactory>(sp =>
    new ActionFactory(
        sp,
        new Dictionary<string, ActionItem>
        {
            ["first"] = new ActionItem(
                typeof(FirstAction).AssemblyQualifiedName!, null),
            ["second"] = new ActionItem(
                typeof(SecondAction).AssemblyQualifiedName!, null)
        }));

services.AddSingleton<IJobQueue, SamplePriorityJobQueue>();

services.AddSingleton<SampleJobProcessor>();
services.AddSingleton<IJobProcessor>(
    sp => sp.GetRequiredService<SampleJobProcessor>());
services.AddHostedService(
    sp => sp.GetRequiredService<SampleJobProcessor>());
```

***

## Start the workflow

```csharp
var job = await jobs.CompactAsync("first");
await jobs.EnqueueJobAsync(job);
```

This triggers the workflow by running the **first action only**.

***

## First action (produces next step)

```csharp
public sealed class FirstAction(
    ILogger<FirstAction> logger,
    IJobProcessor jobs) : IAction
{
    public Task ExecAsync(JsonObject? _, CancellationToken __)
    {
        logger.LogInformation("First action executed");
        return Task.CompletedTask;
    }

    public async Task NextAsync(JsonObject? _, CancellationToken ct)
    {
        var job = await jobs.CompactAsync("second");
        await jobs.EnqueueJobAsync(job);
    }
}
```

*   `ExecAsync` performs the action
*   `NextAsync` schedules the next step in the workflow

***

## Second action (final step)

```csharp
public sealed class SecondAction(
    ILogger<SecondAction> logger) : IAction
{
    public Task ExecAsync(JsonObject? _, CancellationToken __)
    {
        logger.LogInformation("Second action executed");
        return Task.CompletedTask;
    }
}
```

***

## Run

```bash
dotnet run
```

Expected output:

```text
info: WJb.SampleJobProcessor[0]
      JobProcessor started
info: FirstAction[0]
      First action executed
info: SecondAction[0]
      Second action executed
```

***

## Summary

*   Workflow starts with one job
*   Each action can enqueue the next action
*   No external coordinator needed
*   Flow control lives inside actions

This is the **minimal workflow pattern** in WJb.
