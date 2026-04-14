# WJb
[![Nuget](https://img.shields.io/nuget/v/WJb)](https://www.nuget.org/packages/WJb/)

**WJb** is a lightweight and flexible background job execution library for **.NET**, designed for simplicity, control, and predictable behavior.

It focuses on one thing: **structured background job processing without heavy frameworks or hidden magic**.

---

## Requirements

- **Target framework:** .NET 10+
- **API contract:** v0.25 (frozen)

See: docs/api-v0.25.md

---


## What is WJb?

WJb provides a clean execution model for background jobs with:

- Priority-based queues  
- JSON-based payloads  
- Cron-based scheduling  
- Native integration with `IHostedService` and Dependency Injection  

It is ideal for modern .NET applications that need background processing **without infrastructure bloat**.

---

## Who is it for?

- .NET developers who want explicit, predictable behavior  
- Teams that prefer control over conventions  
- Applications where Hangfire or Quartz would be overkill  
- Services requiring structured and efficient background execution  

---

## Features

- ✅ Priority-based job queues  
- ✅ JSON payload handling  
- ✅ Scheduled jobs using standard cron syntax  
- ✅ Seamless integration with `IHostedService`  
- ✅ DI-friendly and extensible architecture  

---

## Samples

WJb provides a small set of focused samples that demonstrate
core API usage and execution semantics.

Advanced production scenarios are intentionally not included in samples.

---

## Sample Engine vs Production Engine

Samples use simplified in-memory implementations to keep behavior
deterministic and easy to understand.

All samples rely **only on public WJb interfaces** and are fully compatible
with the production engine.

When using the production engine:

- Job execution may be parallel  
- Ordering may vary within the same priority level  
- Timing and completion order are not deterministic  

The **meaning and intent** of each sample remain the same.

---

## Licensing

WJb is distributed in two mutually exclusive editions:
- WJb (Apache 2.0)
- WJb.Pro (Commercial)

Both editions share the same public API and namespaces.
The only difference is the NuGet package identifier.
Only one edition may be referenced per application.

### Open Source License
**Apache License 2.0**

This license is suitable for:
- Open-source projects
- Internal or non-commercial usage
- Evaluation and prototyping

### Commercial License
A **Commercial License** is required if you intend to:
- Use WJb in a closed-source commercial product
- Offer it as part of a SaaS or hosted solution
- Redistribute it as part of a paid product or service
- Require enterprise usage, support, or SLA

For commercial licensing inquiries, contact:
📧 **ukrguru@gmail.com**

---

## Philosophy

- Explicit over implicit  
- Simple over clever  
- Control over automation  
- Libraries over frameworks  

---
