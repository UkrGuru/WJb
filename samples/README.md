# WJb Samples

This directory contains **small, focused samples**
demonstrating **core usage patterns** of the **WJb** background job library.

The goal of these samples is to help you:

*   understand the **WJb execution model**
*   learn the **explicit public API**
*   become productive quickly **without hidden behavior**

> ⚠️ These samples are **learning examples**.  
> They are intentionally simplified and are **not production‑ready systems**.

***

## 📦 Structure

    samples/
    ├─ Basics/
    │  ├─ HelloWJb
    │  ├─ PriorityWJb
    │  ├─ WorkflowWJb
    │  └─ CronWJb
    ├─ Advanced/
    │  └─ README.md
    ├─ WJb.Samples.slnx
    └─ README.md

***

## ✅ Included Samples

### Basics

Introductory samples covering essential WJb concepts:

| Sample          | Description                                                   |
| --------------- | ------------------------------------------------------------- |
| **HelloWJb**    | Minimal console application demonstrating first job execution |
| **PriorityWJb** | Priority‑based job ordering                                   |
| **WorkflowWJb** | Explicit job chaining using action‑owned routing              |
| **CronWJb**     | Cron‑based scheduled job execution                            |

Each sample:

*   is fully self‑contained
*   uses `IAction`, `IWorkflowAction`, and DI explicitly
*   runs inside a standard .NET host
*   avoids persistence and infrastructure concerns

***

## 🚫 Advanced Scenarios (Not Included)

The following scenarios are **intentionally excluded**
from public Base‑edition samples:

*   SQL‑backed or distributed queues
*   persistence and retry strategies
*   metrics, dashboards, and monitoring
*   web or UI integrations
*   distributed coordination

```csharp
// Available only in the commercial edition.
```

These topics are either:

*   discussed conceptually, or
*   available through **commercial examples and support**

See `samples/Advanced/README.md` for details.

***

## 🎯 Scope

The samples focus strictly on:

*   WJb public API usage
*   explicit execution flow
*   deterministic behavior
*   action‑owned routing decisions

They deliberately **do not** demonstrate:

*   databases or storage engines
*   retry or backoff policies
*   UI or web frameworks
*   monitoring pipelines
*   production infrastructure concerns

This is intentional.

***

## 🧠 Philosophy

WJb samples emphasize:

*   **clarity over completeness**
*   **explicit behavior over automation**
*   **understanding over abstraction**
*   **API literacy over copy‑paste solutions**

They are designed to explain **how WJb works**,
not to serve as turnkey production templates.

***

## 📦 Package Information

*   **NuGet**: <https://www.nuget.org/packages/WJb>
*   **Namespace**: `WJb`
*   **Target runtime**: .NET 8+ (console apps, hosted services, worker‑style processes)

***

## 📄 Licensing

Samples follow the **same licensing model**
as the main WJb library.

Refer to the root repository README
for complete licensing details.

***
