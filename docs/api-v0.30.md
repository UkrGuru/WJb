# **WJb Public API — v0.30**

This document defines the **frozen public API contract** for **WJb Base**.

***

## Target Framework

*   .NET 8+

***

## Contract Status

*   **Version:** v0.30
*   **Edition:** Base
*   **Status:** Frozen
*   **Backward compatibility:** Guaranteed within v0.30

***

## Terminology

| Term     | Description                                      |
| -------- | ------------------------------------------------ |
| Job      | Serialized unit of work                          |
| Action   | Executable operation identified by a string code |
| Payload  | UTF‑8 encoded JSON string                        |
| Snapshot | Immutable view of registered actions             |

***

## Action Definition

### `ActionItem`

```csharp
public class ActionItem
{
    public string Type { get; set; }
    public JsonObject? More { get; set; }

    public ActionItem();
    public ActionItem(string type, JsonObject? more);
}
```

***

## Actions

### `IAction`

```csharp
public interface IAction
{
    Task ExecAsync(JsonObject? payload, CancellationToken cancellationToken);
}
```

***

## Action Registry

### `IActionRegistry`

```csharp
public interface IActionRegistry
{
    IReadOnlyDictionary<string, ActionItem> Snapshot();
}
```

### `IActionFactory`

```csharp
public interface IActionFactory : IActionRegistry
{
    IAction Create(string actionCode);
    ActionItem GetActionItem(string actionCode);
}
```

***

## Job Payload Format

```json
{
  "code": "action.code",
  "more": { }
}
```

*   `code` — required action identifier
*   `more` — optional metadata object

***

## Job Processing

### `IJobProcessor`

```csharp
public interface IJobProcessor
{
    Task<string> CompactAsync(
        string actionCode,
        object? jobMore = null,
        CancellationToken stoppingToken = default);

    Task EnqueueJobAsync(
        string job,
        Priority priority = Priority.Normal,
        CancellationToken stoppingToken = default);

    Task<(string Code, JsonObject More)> ExpandAsync(
        string job,
        CancellationToken stoppingToken = default);

    Task ProcessJobAsync(
        string job,
        CancellationToken stoppingToken = default);
}
```

***

## Job Queue

### `IJobQueue`

```csharp
public interface IJobQueue
{
    Task EnqueueAsync(
        string job,
        Priority priority,
        CancellationToken cancellationToken = default);

    Task<string> DequeueNextAsync(
        CancellationToken cancellationToken = default);
}
```

***

## Scheduler

### `IJobScheduler`

```csharp
public interface IJobScheduler
{
    Task ProcessDueCronActionsAsync(
        DateTime now,
        CancellationToken cancellationToken = default);

    bool IsDue(string? cron, DateTime now);
}
```

***

## Workflow Routing

### `IWorkflowAction`

```csharp
public interface IWorkflowAction
{
    Task NextAsync(
        JsonObject? nextMore,
        CancellationToken stoppingToken = default);
}
```

***

## Priority

```csharp
public enum Priority
{
    ASAP,
    High,
    Normal,
    Low
}
```

***

## Metadata (`JsonObject`)

*   Free‑form JSON
*   No schema enforcement
*   Interpreted only by actions
*   Passed unchanged through execution

***

## Snapshots

*   Immutable
*   Thread‑safe
*   Deterministic
*   Atomically replaceable

***

## Execution Semantics

*   Explicit execution
*   No implicit orchestration
*   No retries
*   No persistence
*   No background guarantees

***

## Thread Safety

*   No external synchronization required
*   Reference replacement over locking
*   Eventual consistency assumed

***

## Non‑Goals

*   Delivery guarantees
*   Retries or backoff
*   Persistent storage
*   Dynamic reload
*   Runtime mutation
*   Worker orchestration
*   Settings registry

***

## Compatibility Guarantee

Within **v0.30**:

*   Method signatures are stable
*   Payload format is stable
*   Semantics are stable
