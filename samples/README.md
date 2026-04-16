# WJb Samples

This directory contains **small, focused samples** that demonstrate **core usage patterns** of the **WJb** background job library.

The goal of these samples is to help you:

*   understand the **WJb execution model**
*   learn the **explicit APIs**
*   get productive quickly **without hidden magic**

> ⚠️ These are **learning examples**, not production-ready systems.

***

## 📦 Structure

    samples/
    ├─ Basics/
    │  ├─ HelloWJb
    │  ├─ PriorityWJb
    │  └─ WorkflowWJb
    ├─ Advanced/
    │  └─ README.md
    ├─ WJb.Samples.slnx
    └─ README.md

***

## ✅ Included Samples

### **Basics**

Introductory samples covering the essential WJb concepts:

| Sample          | Description                                              |
| --------------- | -------------------------------------------------------- |
| **HelloWJb**    | Minimal console app showing first job execution          |
| **PriorityWJb** | Using priority-based queues                              |
| **WorkflowWJb** | Job chaining via `NextAsync` (explicit workflow routing) |

Each sample:

*   is fully self-contained
*   uses `IAction`, `JobProcessor`, and DI explicitly
*   avoids infrastructure and persistence concerns

***

## 🚫 Advanced Scenarios (Not Included)

Some advanced scenarios are **intentionally excluded** from public samples:

*   SQL-backed queues
*   Metrics & dashboards
*   Persistence & retries
*   Web / UI integrations
*   Distributed coordination

```csharp
// Available only in the commercial edition.
```

These topics are either:

*   covered conceptually, or
*   available as part of **commercial examples and support**

See `samples/Advanced/README.md` for details. [\[github.com\]](https://github.com/UkrGuru/WJb/tree/main/samples/Advanced)

***

## 🎯 Scope

The samples focus strictly on:

*   WJb API usage
*   execution flow
*   deterministic behavior
*   developer control

They **do not** demonstrate:

*   databases
*   retries
*   UI apps
*   monitoring
*   production infrastructure

This is by design.

***

## 🧠 Philosophy

WJb samples emphasize:

*   **clarity over completeness**
*   **explicit behavior over magic**
*   **understanding over abstractions**
*   **API literacy over copy/paste solutions**

This keeps both the samples **honest** and the library **lightweight**. [\[github.com\]](https://github.com/UkrGuru/WJb/tree/main/samples)

***

## 📦 Package Info

*   **NuGet**: <https://www.nuget.org/packages/WJb>
*   **Namespace**: `WJb`
*   **Target**: .NET (Hosted Services, Console, Worker-style apps)

***

## 📄 Licensing

Samples follow the **same license model** as the main WJb library.

Refer to the **root repository README** for licensing details.

***
