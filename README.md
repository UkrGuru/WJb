## WJb

[![Nuget](https://img.shields.io/nuget/v/WJb)](https://www.nuget.org/packages/WJb/)

**WJb** is a lightweight and explicit background job execution library for **.NET**.

It is designed for developers who prefer **predictable behavior, explicit execution flow,
and full control** over background job processing — without infrastructure-heavy frameworks
or hidden runtime magic.

WJb focuses on one thing:

**Structured background job execution with clear and limited guarantees.**

---

## Requirements

- **Target framework:** .NET 8+
- **API contract:** v0.28 (frozen)  
  See: `docs/api-v0.28.md`

---

## What is WJb?

WJb provides a minimal and explicit execution model for background jobs and workflows, including:

- Priority-based job queues
- JSON-based payloads
- Cron-based scheduling
- Job chaining and workflow-style execution
- Native integration with `IHostedService` and Dependency Injection

It is intended for applications that require background processing
without introducing distributed infrastructure or opinionated frameworks.

---

## Who is WJb for?

- .NET developers who value explicit behavior over conventions
- Teams that want full control over job execution semantics
- Applications where Hangfire or Quartz would be excessive
- Services that require predictable and testable background execution

---

## Features

- ✅ Priority-based job queues
- ✅ JSON payload handling
- ✅ Cron-based scheduling
- ✅ Workflow chaining
- ✅ Seamless integration with `IHostedService`
- ✅ DI-friendly and extensible architecture

---

## Samples

WJb provides a small set of focused samples that demonstrate the public API
and execution semantics.

Advanced production scenarios are intentionally excluded from samples.
All samples are:
- deterministic
- easy to understand
- representative of real execution behavior

---

## Sample Engine vs Production Engine

Samples use simplified in-memory implementations to keep behavior deterministic.

All samples rely **only on public WJb interfaces** and are fully compatible
with the production engine.

In production environments:
- Job execution may be parallel
- Ordering may vary within the same priority level
- Execution timing is not deterministic

The **intent and meaning** of each sample remain unchanged.

---

## Execution model & guarantees

WJb follows a **minimal and explicit execution model**.
To avoid incorrect assumptions, guarantees are intentionally limited.

### What is guaranteed

- Jobs are executed **at least once**
- Ordering is guaranteed **only within a single workflow chain**
- Workflow progression is driven by declarative metadata (`ActionItem.More`)

### What is NOT guaranteed

- Exactly-once execution
- Global ordering across workflows
- Deterministic ordering for jobs of the same priority
- Execution timing under load

### Notes on parallelism

- Parallel execution depends on the configured job queue
- Production queues may reorder jobs of equal priority
- **Actions must be idempotent and safe for re-execution**

If strict ordering, retries, or transactional guarantees are required,
they must be handled explicitly by application logic.

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
- Control over automation
- Libraries over frameworks

---

© 2025–2026 Oleksandr Viktor (UkrGuru). All rights reserved.
``