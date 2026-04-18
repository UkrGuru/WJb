# 📘 WJb Public API — v0.27.0 (Free Edition)

> **Status:** Frozen  
> **Target runtime:** .NET 8+  
> **Edition:** Free (Apache 2.0)

This document defines the **public and supported API surface**
of **WJb v0.27.0 Free edition**.

Any API not listed here is considered **internal** and may change
without notice.

***

## 📦 Namespaces

All public types are defined in the following namespaces:

    WJb
    WJb.Extensions

***

## 🔹 Core Concepts

WJb is based on four core abstractions:

1.  **Actions** — executable job logic
2.  **Action registry** — declarative action configuration
3.  **Job processor** — job creation and execution
4.  **Job queue & scheduler** — background processing infrastructure

***

## 🧩 IAction

```csharp
public interface IAction
{
    Task ExecAsync(
        JsonObject? jobMore,
        CancellationToken cancellationToken);
}
```

### Description

Represents a single executable job action.

### Notes

*   Actions **must be idempotent**
*   `jobMore` contains job payload
*   Actions are resolved via Dependency Injection

***

## 🧩 ActionItem

```csharp
public sealed class ActionItem
{
    public required string Type { get; init; }
    public JsonObject? More { get; init; }
}
```

### Description

Declarative metadata describing an action.

*   `Type` — assembly-qualified CLR type name
*   `More` — optional action configuration metadata

***

## 🧩 IActionRegistry

```csharp
public interface IActionRegistry
{
    IReadOnlyDictionary<string, ActionItem> Snapshot();
}
```

### Description

Read-only registry of action definitions.

### Notes

*   No mutation or reload capabilities
*   Runtime configuration control is not available in Free edition

***

## 🧩 IActionFactory

```csharp
public interface IActionFactory : IActionRegistry
{
    IAction Create(string actionType);
    ActionItem GetActionItem(string actionCode);
}
```

### Description

Factory responsible for action instantiation and metadata access.

***

## 🧩 IJobProcessor

```csharp
public interface IJobProcessor
{
    Task<JsonObject> CompactAsync(
        string actionCode,
        object? jobMore = null,
        CancellationToken cancellationToken = default);

    Task EnqueueJobAsync(
        JsonObject job,
        CancellationToken cancellationToken = default);
}
```

### Description

High-level API for job creation and execution.

### Notes

*   `CompactAsync` creates a job payload
*   `EnqueueJobAsync` submits a job for background execution
*   Job execution is handled internally via a hosted service

***

## 🧩 IJobQueue

```csharp
public interface IJobQueue
{
    Task EnqueueAsync(
        string job,
        Priority priority,
        CancellationToken cancellationToken = default);

    Task<(string Job, Priority Priority)> DequeueNextAsync(
        CancellationToken cancellationToken = default);
}
```

### Description

Priority-aware job queue abstraction.

### Guarantees

*   **Strict priority ordering**
*   **FIFO within the same priority**
*   Starvation of lower priorities is allowed

### Notes

*   No slot management
*   No runtime tuning
*   No release semantics in Free edition

***

## 🧩 IJobScheduler

```csharp
public interface IJobScheduler
{
    Task IntervalDelayAsync(
        DateTime now,
        CancellationToken stoppingToken);

    bool IsDue(
        string? cron,
        DateTime now);
}
```

### Description

Time-based scheduling abstraction.

### Notes

*   Cron evaluation only
*   No configuration reload support in Free edition

***

## 🔹 Dependency Injection

### AddWJb

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWJb(
        this IServiceCollection services,
        IDictionary<string, ActionItem> actions);
}
```

### Description

Registers all required WJb services.

### Notes

*   Registers job processor as a hosted service
*   Automatically wires action resolution
*   No additional configuration is required

***

## ⚠️ Explicit Exclusions (Not Present in Free)

The following capabilities are **not available** in v0.27.0 Free edition:

*   Runtime configuration reload
*   Queue slot management
*   Live scheduler tuning
*   Any `IReloadable*` interfaces

These capabilities are provided **only in the Commercial edition**.

***

## ✅ Stability Guarantees

*   This API surface is **frozen** for v0.27.x
*   Breaking changes will require a **minor version bump**
*   Internal types may change without notice

***

## 🏁 Summary

WJb v0.27.0 Free provides:

*   A minimal and explicit background job execution model
*   High-performance enqueue semantics
*   Strict and well-defined queue guarantees
*   Clean and dependency-injection-friendly API

No hidden behavior. No runtime magic. No implicit control.

***

