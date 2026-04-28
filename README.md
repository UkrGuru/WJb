# WJb — Background Job Architecture for .NET
[![NuGet](https://img.shields.io/nuget/v/WJb)](https://www.nuget.org/packages/WJb/)

## Overview

**WJb** is a lightweight, deterministic background job system for .NET,
designed for applications where background execution is part of the *core domain*
rather than a side concern.

WJb favors **explicit execution**, **predictable behavior**, and **long‑term
API stability** over convenience features like hidden retries, automatic pipelines,
or implicit orchestration.

It is well‑suited for backend systems where correctness, debuggability, and
operational clarity matter more than configurability.

---

## Key Characteristics

- Explicit job payloads (no ambient state)
- Queue‑first execution model
- Deterministic, single‑step execution
- Immutable metadata snapshots
- No hidden retries or background pipelines
- No implicit orchestration logic
- Stable, contract‑based public API

---

## Design Philosophy

WJb is built around a few core ideas:

- **Actions control their own execution lifecycle**
- **Routing is explicit and owned by the action**
- **Infrastructure executes — domain logic orchestrates**
- **Metadata is configuration, not state**
- **Nothing happens implicitly**

If a job runs, it is because someone explicitly enqueued it.
If a workflow continues, it is because an action explicitly decided so.

---

## What WJb Is *Not*

WJb intentionally does **not** provide:

- Automatic retries
- Background pipelines
- Hidden chaining
- Distributed locks
- Persistent orchestration engines

If your system needs those features, WJb is probably not the right tool —
and that is a deliberate design choice.

---

## Documentation

- **Architecture & execution model**  
  → [README.architecture.md](README.architecture.md)

- **Design decisions & trade‑offs**  
  → [README.design.md](README.design.md)

- **Commercial usage & collaboration**  
  → [README.commercial.md](README.commercial.md)

- **About the author**  
  → [README.author.md](README.author.md)

---

## Status

- Target framework: **.NET 8+**
- API contract version: **v0.29 (frozen)**
- Free edition: stable
- Commercial edition: available separately (WJb.Pro)

---

## Licensing

WJb is dual‑licensed:

- **Apache License 2.0**  
  For open‑source and non‑commercial usage

- **Commercial License (WJb.Pro)**  
  Required for closed‑source, SaaS, and commercial distribution

For commercial licensing inquiries, see [README.commercial.md](README.commercial.md).

---

## Philosophy in One Sentence

> **WJb trades convenience for clarity — so that background code behaves like any other code you trust.**

---

## ❤️ Support

If this project helps you, you can support its development:
👉 https://ko-fi.com/ukrguru
