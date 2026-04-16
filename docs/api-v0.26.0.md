## WJb API v0.26.0

This document describes the **public API and execution model of WJb v0.26.0**. It is intended as the **final, release-ready specification** and reflects the behavior of the `dev` branch at the moment of release.

WJb is designed around **explicit behavior, predictable execution, and minimal magic**.

---

## 1. Overview

**WJb** is a lightweight background job execution library for .NET. It executes **user-defined actions** using a **priority-based queue**, **JSON payloads**, and **explicit job continuation**.

Key characteristics:

- No database required (in-memory queue by default)
- Priority-based execution
- JSON-first payload model
- Explicit dependency injection
- Predictable execution flow
- Optional scheduling and hosted services

WJb integrates naturally with **ASP.NET Core**, **console applications**, and **worker services** via `IHostedService`.

---

## 2. Core Concepts

### Job

A **job** is a unit of work defined by:

- **Action code** (`string`)
- **Payload** (`JsonObject`)
- **Priority**
- **Optional chaining metadata**

Jobs are enqueued into an `IJobQueue` and processed sequentially by a `JobProcessor`.

Jobs may optionally enqueue subsequent jobs, enabling **workflow-like execution** without a separate orchestration engine.

---

### Action

An **action** is a user-defined CLR type that performs work:

- Stateless by design
- Registered as **Transient**
- Resolved through Dependency Injection

Actions are mapped by **action code → CLR type** using `ActionItem` metadata.

---

### Action Chaining

An action may define continuation metadata in its payload. After execution:

- The processor evaluates continuation rules
- A next job may be enqueued
- The decision may depend on execution success or failure

This model enables deterministic, code-driven workflows while preserving a simple queue-based architecture.

---

### Action Factory (IActionFactory)

The action factory is responsible for:

- Creating action instances
- Resolving actions by code (case-insensitive)
- Providing immutable snapshots of action metadata

The factory is registered as a **Singleton**.

---

## 3. Job Processing Model

### JobProcessor

`JobProcessor`:

- Dequeues jobs respecting priority order
- Expands JSON payloads into action metadata
- Executes actions via the factory
- Handles continuation logic

Execution guarantees:

- Jobs are processed sequentially per processor instance
- Cancellation is cooperative
- Continuation handling errors do not break the processing loop

Parallelism is achieved by running multiple processors with separate queues.

---

## 4. Dependency Injection Extensions

Namespace: `WJb.Extensions`

### AddWJb(...)

```csharp
IServiceCollection AddWJb(
    IDictionary<string, ActionItem>? actions = null,
    bool addActionFactory = true,
    bool addProcessor = true,
    bool addScheduler = false,
    bool addHostedServices = true)
```

Registers the complete WJb runtime.

---

## 5. Scheduling (Optional)

When enabled, WJb supports time-based job scheduling (e.g., cron expressions).

---

## 7. References

- Repository: https://github.com/UkrGuru/WJb
- Samples: https://github.com/UkrGuru/WJb/tree/main/samples

**End of WJb API v0.26.0**
