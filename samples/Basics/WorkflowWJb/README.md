# Workflow WJb – Minimal Example

This example demonstrates a **minimal job workflow** using **WJb**, where the execution order is defined declaratively and each action can automatically enqueue the next action.

The example is intentionally compact and uses the modern `AddWJb(...)` registration style.

---

## What happens

1. A generic host is started
2. A workflow definition is registered via `ActionItem`
3. The first job (`first`) is compacted and enqueued
4. `FirstAction` runs
5. The workflow automatically schedules `SecondAction`
6. `SecondAction` runs

Execution order is strictly controlled by the workflow configuration.

---

## Workflow definition

Actions and their relationships are defined up-front:

```csharp
var actions = new Dictionary<string, ActionItem>
{
    ["first"] = new ActionItem
    {
        Type = "FirstAction, WorkflowWJb",
        More = new JsonObject { ["next"] = "second" }
    },
    ["second"] = new ActionItem
    {
        Type = "SecondAction, WorkflowWJb"
    }
};
```

- `Type` specifies the action implementation
- `More["next"]` declares the next step in the workflow
- No external coordinator is required

---

## Service registration

```csharp
services.AddWJb(actions);
```

`AddWJb` registers:
- the job processor
- the action factory
- workflow metadata
- the hosted background service

---

## Starting the workflow

```csharp
var jobs = host.Services.GetRequiredService<IJobProcessor>();

var job = await jobs.CompactAsync("first");
await jobs.EnqueueJobAsync(job);
```

Only the **first job** is enqueued manually. All subsequent steps are derived from the workflow definition.

---

## Actions

### First action

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
}
```

### Second action

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

- Actions only implement `ExecAsync`
- Workflow progression is driven by metadata (`More.next`), not code

---

## Run

```bash
dotnet run
```

Expected output:

```text
JobProcessor started
First action executed
Second action executed
```

---

## Summary

- The workflow starts with a single job
- Each action automatically schedules the next one
- No manual chaining logic inside actions
- Flow control lives in declarative workflow metadata

This is the **simplest recommended workflow pattern** in WJb.
