# WJb API v0.26.0

This document describes the **public API and execution model of WJb v0.26.0**, based on the current `dev` branch of the repository and aligned with all samples and extensions provided by the library. The focus is on **explicit behavior, predictable lifecycle, and minimal magic**, which are core design goals of WJb.

---

## 1. Overview

**WJb** is a lightweight background job execution library for .NET. It executes **user-defined actions** using a **priority-based queue**, **JSON payloads**, and **explicit chaining** between jobs.

Key characteristics:

- No database required (in-memory queue by default)
- Explicit dependency injection
- Predictable execution flow
- JSON-first payload model
- Optional scheduling and hosted services

WJb integrates naturally with **ASP.NET Core**, **console apps**, and **worker services** through `IHostedService`.

---

## 2. Core Concepts

### Job

A job is a unit of work defined by:

- **Action code** (string)
- **Payload (`JsonObject`)**
- **Priority**
- **Optional chaining metadata**

Jobs are enqueued into an `IJobQueue` and processed sequentially by a `JobProcessor`.

---

### Action

An **action** is a user-defined CLR type that performs work:

- Stateless
- Registered as `Transient`
- Resolved through DI

Actions are mapped by **action code → CLR type** via `ActionItem` metadata.

---

### Action Factory (`IActionFactory`)

Responsible for:

- Resolving action instances
- Providing immutable snapshots of action metadata
- Supporting case-insensitive action codes

The factory is registered as a **singleton**.

---

## 3. Dependency Injection Extensions

Namespace: `WJb.Extensions`

### `AddWJb(...)`

````csharp
IServiceCollection AddWJb(
    IDictionary<string, ActionItem>? actions = null,
    bool addActionFactory = true,
    bool addProcessor = true,
    bool addScheduler = false,
    bool addHostedServices = true)
````

Registers the complete WJb runtime.

---

## 11. References

- WJb Repository: https://github.com/UkrGuru/WJb
- Samples: https://github.com/UkrGuru/WJb/tree/main/samples

---

**End of api-v0.26.0.md**
