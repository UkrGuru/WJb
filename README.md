# WJb — Background Job Architecture for .NET

[![NuGet](https://img.shields.io/nuget/v/WJb)](https://www.nuget.org/packages/WJb/)

## Overview

**WJb** is a lightweight, deterministic background job execution library for .NET.

It provides a **minimal execution core** for applications where background
processing is part of **explicit domain logic**, not an infrastructural side effect.

WJb prioritizes **explicit execution**, **predictable behavior**, and
**long‑term API stability** over convenience features or implicit orchestration.

***

## Quick example (explicit workflow)

A minimal workflow where each step is defined declaratively  
and only the first job is enqueued manually.

```csharp
var actions = new Dictionary<string, ActionItem>
{
    ["first"] = new()
    {
        Type = "FirstAction, Sample",
        More = new JsonObject { ["next"] = "second" }
    },
    ["second"] = new()
    {
        Type = "SecondAction, Sample"
    }
};

services.AddWJb(actions);

var jobs = host.Services.GetRequiredService<IJobProcessor>();
var job = await jobs.CompactAsync("first");
await jobs.EnqueueJobAsync(job);
```

Only the first job is enqueued explicitly.  
All further workflow progression is driven by declared metadata.

A full runnable example is available at:  
→ `samples/Basics/WorkflowWJb`

***

## Scope of the Base Edition

This package represents the **Base (free) edition** of WJb.

It is intentionally limited to **job execution primitives** and
does **not** include advanced runtime features available in the commercial edition.

The Base edition is suitable for:

*   Open‑source projects
*   Learning and evaluation
*   Small or embedded workers
*   Deterministic background execution

***

## Key Characteristics

*   Explicit job payloads (no ambient state)
*   Queue‑first execution model
*   Deterministic, single‑step processing
*   Immutable action metadata snapshots
*   Explicit workflow routing
*   Stable, contract‑based public API

***

## Base vs Commercial Comparison

| Area                          | **WJb Base**                      | **WJb Commercial (WJb.Pro)**                      |
| ----------------------------- | --------------------------------- | ------------------------------------------------- |
| Product name                  | WJb                               | WJb                                               |
| Package                       | `WJb`                             | `WJb.Pro`                                         |
| License                       | Apache License 2.0                | WJb — Commercial Capability License (Solo / Team) |
| Intended use                  | Open‑source, learning, evaluation | Commercial, SaaS, production, closed‑source       |
| Job execution                 | ✅ Explicit, deterministic         | ✅ Explicit, deterministic                         |
| Job payloads                  | ✅ JSON, explicit                  | ✅ JSON, explicit                                  |
| Execution model               | ✅ Queue‑first                     | ✅ Queue‑first                                     |
| Action model                  | ✅ `IAction`, explicit routing     | ✅ Same, extended capabilities                     |
| Workflow routing              | ✅ Explicit (`IWorkflowAction`)    | ✅ Same, plus extensions                           |
| Scheduling (cron)             | ✅ Basic                           | ✅ Extended                                        |
| Settings registry             | ❌ Not available                   | ✅ Available                                       |
| Runtime reload                | ❌ Not supported                   | ✅ Supported                                       |
| Persistence                   | ❌ In‑memory only                  | ✅ Optional implementations                        |
| Delivery guarantees           | ❌ None                            | ✅ Optional / configurable                         |
| Advanced orchestration        | ❌ Not provided                    | ✅ Available                                       |
| Production hardening features | ❌ Minimal                         | ✅ Included                                        |
| Author support                | ❌ Community only                  | ✅ Direct author support                           |
| Commercial redistribution     | ❌ Not permitted                   | ✅ Permitted under license                         |

***

### Notes

*   **WJb Base** is intentionally minimal and deterministic.  
    If a feature is not listed, it does not exist.
*   **WJb.Pro** does not change the execution philosophy — it **extends** it.
*   Both editions share the same core concepts and API shape.
*   Migration from Base to Commercial does **not** require rewriting domain logic.

***

## Design Philosophy

WJb is built around a small set of strict principles:

*   **Actions control their own execution lifecycle**
*   **Routing is explicit and action‑owned**
*   **Infrastructure executes — domain logic orchestrates**
*   **Metadata is configuration, not state**
*   **Nothing happens implicitly**

If a job runs, it was explicitly enqueued.  
If a workflow continues, an action explicitly decided so.

***

## What WJb Is *Not*

WJb intentionally does **not** provide:

*   Automatic retries
*   Background pipelines
*   Implicit chaining
*   Distributed locks
*   Persistence guarantees
*   Workflow engines
*   Runtime reload or mutation
*   Settings registry (Base edition)

These omissions are deliberate design decisions.

***

## Documentation

*   **Architecture & execution model**  
    → README.architecture.md

*   **Design decisions & trade‑offs**  
    → README.design.md

*   **Commercial edition & licensing**  
    → README.commercial.md

*   **About the author**  
    → README.author.md

***

## Status

*   Target framework: **.NET 8+**
*   API contract version: **v0.30**
*   Edition: **Base**
*   Commercial edition: **WJb.Pro**

***

## License Types

The commercial capabilities of **WJb** are available under the  
**WJb — Commercial Capability License**.

* **Solo License** — for individual developers using WJb in commercial projects  
  → https://ukrguru.gumroad.com/l/wjb-solo-lic

The **Base edition** remains free for non-commercial use, evaluation,
and open-source projects.

For full commercial terms, redistribution rules, and license details, see:  
→ `README.commercial.md`

***

## Licensing

The **Base edition** of WJb is available under the **Apache License 2.0**.

Commercial, SaaS, and closed‑source usage requires the appropriate
commercial license.

***

## Philosophy in One Sentence

> **WJb treats background execution as explicit domain code — predictable, traceable, and debuggable.**

***

## ❤️ Support

If this project helps you, you can support its development:  
👉 <https://ko-fi.com/ukrguru>
