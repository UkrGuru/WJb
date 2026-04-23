# WJb

**WJb** is a lightweight, extensible background job processor for .NET, designed around
explicit job payloads, immutable snapshots, and deterministic scheduling.

The project focuses on **predictable behavior**, **low abstraction overhead**, and
**long-term API stability**.

---

## Status

- **Target framework:** .NET 8+
- **API contract:** v0.28 (**frozen**)
- **Branch:** `dev`
- **Current version:** v0.28

API documentation:
👉 docs/api-v0.28.md

---

## Key Concepts

- **Job payload** — a compact JSON string representing an action and its metadata.
- **Queue-first architecture** — jobs are always enqueued explicitly.
- **Immutable snapshots** — readers always see a consistent system state.
- **Cron scheduling** — optional cron metadata attached to actions.
- **Explicit execution flow** — no implicit retries or hidden pipelines.

---

## Design Principles

- No magic background threads
- No reflection-driven dispatch
- No hidden dependency injection tricks
- Immutable data instead of locks
- Explicit > implicit

---

## Example

```csharp
var payload = await jobSerializer.CompactAsync(
    actionCode: "email.send",
    jobMore: new
    {
        to = "user@example.com",
        subject = "Hello"
    },
    stoppingToken);

await jobQueue.EnqueueJobAsync(payload, Priority.Normal, stoppingToken);
````

***

## Versioning

WJb uses **contract-based versioning**.

*   Patch releases may include bug fixes.
*   Minor versions may extend functionality **without breaking** the API contract.
*   Major versions may redefine contracts.

The **v0.28 API is frozen** and will remain backward-compatible.

***

## Licensing

WJb is available under **dual licensing**.

### Open Source License (Free Edition)

**Apache License 2.0**

Suitable for:

*   Open-source projects
*   Internal or non-commercial usage
*   Evaluation and prototyping

***

### Commercial License (WJb.Pro)

A **Commercial License** is required if you intend to:

*   Use WJb in a closed-source commercial product
*   Offer WJb as part of a SaaS or hosted solution
*   Redistribute it as part of a paid product or service
*   Require enterprise usage, support, or SLA

For commercial licensing inquiries:  
📧 **<ukrguru@gmail.com>**

```

---
