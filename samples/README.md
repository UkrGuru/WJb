# WJb Samples

This directory contains **small, focused samples**
that demonstrate **core usage patterns** of the **WJb** background job library.

The goal of these samples is to help you:

- understand the **WJb execution model**
- learn the **explicit public APIs**
- become productive quickly **without hidden magic**

> ⚠️ These samples are **learning examples**.
> They are intentionally simplified and are not production‑ready systems.

---

## 📦 Structure

```

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

```

---

## ✅ Included samples

### Basics

Introductory samples covering essential WJb concepts:

| Sample | Description |
|------|------------|
| **HelloWJb** | Minimal console application showing first job execution |
| **PriorityWJb** | Using priority‑based job ordering |
| **WorkflowWJb** | Explicit job chaining using action‑owned routing |
| **CronWJb** | Cron‑based scheduled job execution |

Each sample:

- is fully self‑contained
- uses `IAction`, `WorkflowActionBase`, and DI explicitly
- runs through a standard .NET host
- avoids persistence and infrastructure concerns

---

## 🚫 Advanced scenarios (not included)

Some advanced scenarios are **intentionally excluded**
from public Free‑edition samples:

- SQL‑backed or distributed queues
- persistence and retries
- metrics, dashboards, and monitoring
- web / UI integrations
- distributed coordination

```csharp
// Available only in the commercial edition.
```

These topics are either:

*   discussed conceptually, or
*   available as part of **commercial examples and support**

See `samples/Advanced/README.md` for details.  
<https://github.com/UkrGuru/WJb/tree/main/samples/Advanced>

***

## 🎯 Scope

The samples focus strictly on:

*   WJb public API usage
*   explicit execution flow
*   deterministic behavior
*   developer‑controlled routing

They deliberately do **not** demonstrate:

*   databases or storage engines
*   retry or backoff policies
*   UI or web applications
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

The samples are meant to explain *how WJb works*,
not to serve as turnkey production templates.

***

## 📦 Package information

*   **NuGet**: <https://www.nuget.org/packages/WJb>
*   **Namespace**: `WJb`
*   **Target runtime**: .NET (hosted services, console apps, worker‑style services)

***

## 📄 Licensing

Samples follow the **same licensing model**
as the main WJb library.

Refer to the root repository README
for full licensing details.

***
