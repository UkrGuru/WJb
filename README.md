## WJb

[![Nuget](https://img.shields.io/nuget/v/WJb)](https://www.nuget.org/packages/WJb/)

**WJb** is a lightweight and explicit background job execution library for **.NET 8+**.
It is designed for predictability, control, and minimal abstraction — without heavy frameworks
or hidden runtime magic.

WJb focuses on one thing:

> **Structured background job processing with an explicit and honest execution model.**

---

## Requirements

- **Target framework:** .NET 8+
- **API contract:** v0.27 (frozen)

### Platform Requirements by Edition

- **WJb (Free edition):** .NET 8 or higher
- **WJb.Pro (Commercial edition):** .NET 10 only

The Commercial edition intentionally targets a newer runtime
to support advanced runtime control features and future platform capabilities.

---

## What is WJb?

WJb provides a clean execution model for background jobs and workflows with:

- Priority-based queues
- JSON-based job payloads
- Cron-based scheduling
- Declarative job chaining and workflow-like execution
- Native integration with `IHostedService` and Dependency Injection

It is intended for modern .NET applications that require background processing
**without infrastructure bloat or implicit behavior**.

---

## Who is it for?

- .NET developers who want **explicit and predictable behavior**
- Teams that prefer **capabilities over conventions**
- Applications where Hangfire or Quartz are unnecessary or excessive
- Services that require **controlled background execution**

---

## Features (Free Edition)

- ✅ Priority-based job queues
- ✅ JSON payload handling
- ✅ Scheduled jobs using standard cron syntax
- ✅ Workflow-style job chaining
- ✅ Seamless integration with `IHostedService`
- ✅ DI-friendly and extensible architecture

---

## Free vs Commercial Edition

WJb is distributed in two mutually exclusive editions:

- **WJb** — Free edition (Apache 2.0)
- **WJb.Pro** — Commercial edition

### Free Edition

The Free edition provides the **core execution model**:

- Job scheduling
- Job queues
- Job execution and chaining
- Stable, explicit core interfaces

It is fully functional for running background jobs
and **does not contain any runtime control or live reconfiguration features**.

### Commercial Edition

The Commercial edition extends the core API with **explicit runtime control capabilities**,
including:

- Live configuration reload
- Queue tuning and runtime slot management
- Advanced operational control

These capabilities are exposed via dedicated interfaces
(`IReloadable*`) that are **not present in the Free edition**.

Only one edition may be referenced per application.

---

## Samples

WJb provides a small set of focused samples that demonstrate
core API usage and execution semantics.

Advanced production scenarios are intentionally not included in samples.

---

## Sample Engine vs Production Engine

Samples use simplified in-memory implementations to keep behavior
deterministic and easy to understand.

All samples rely **only on public WJb interfaces**
and are fully compatible with the production engine.

In production environments:

- Job execution may be parallel
- Ordering may vary within the same priority level
- Timing and completion order are not deterministic

The **meaning and intent** of each sample remain the same.

---

## Licensing

### Open Source License (Free Edition)

**Apache License 2.0**

Suitable for:

- Open-source projects
- Internal or non-commercial usage
- Evaluation and prototyping

### Commercial License (WJb.Pro)

A Commercial License is required if you intend to:

- Use WJb in a closed-source commercial product
- Offer WJb as part of a SaaS or hosted solution
- Redistribute it as part of a paid product or service
- Require enterprise usage, support, or SLA

For commercial licensing inquiries:
📧 **ukrguru@gmail.com**

---

## Philosophy

- Explicit over implicit
- Simple over clever
- Capabilities over conventions
- Libraries over frameworks

---

## Execution Model & Guarantees

WJb follows a **minimal and explicit execution model**.
To avoid incorrect assumptions, guarantees are intentionally limited.

### What is guaranteed

- Jobs are executed **at least once**
- Ordering is guaranteed **only within a single workflow chain**
  (e.g. first → second → third)
- Workflow progression is driven by declarative metadata (`ActionItem.More`)

### What is NOT guaranteed

- Exactly-once execution
- Global ordering across different workflows
- Deterministic ordering between jobs of the same priority
- Exact execution timing under load

### Notes on parallelism

- Parallel execution depends on the configured job queue
- Production queues may reorder jobs of equal priority
- Actions **must be idempotent** and safe for re-execution

If your use case requires strict ordering, retries, or transactional guarantees,
those concerns must be handled explicitly by application logic.
