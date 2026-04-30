# WJb — Background Job Architecture for .NET

[![NuGet](https://img.shields.io/nuget/v/WJb)](https://www.nuget.org/packages/WJb/)

## Overview

**WJb** is a lightweight, deterministic background job execution library for .NET.

It provides a **minimal execution core** for applications where background
processing is part of the *domain logic*, not an infrastructural afterthought.

WJb prioritizes **explicit execution**, **predictable behavior**, and
**long‑term API stability** over convenience features or implicit orchestration.

***

## Scope of the Free Edition

This package represents the **free core** of WJb.

It is intentionally limited to background job **execution mechanics** and
does **not** include advanced features provided by the commercial edition.

The free edition is suitable for:

*   open‑source projects
*   learning and evaluation
*   non‑commercial usage

***

## Key Characteristics

*   Explicit job payloads (no ambient state)
*   Queue‑first execution model
*   Deterministic, single‑step execution
*   Immutable metadata snapshots
*   No implicit orchestration logic
*   Stable, contract‑based public API

***

## Design Philosophy

WJb is built around a small set of strict principles:

*   **Actions control their own execution lifecycle**
*   **Routing is explicit and owned by the action**
*   **Infrastructure executes — domain logic orchestrates**
*   **Metadata is configuration, not state**
*   **Nothing happens implicitly**

If a job runs, it is because it was explicitly enqueued.  
If a workflow continues, it is because an action explicitly decided so.

***

## What WJb Is *Not*

WJb intentionally does **not** provide:

*   Automatic retries
*   Background pipelines
*   Hidden chaining
*   Distributed locks
*   Persistent orchestration engines

These omissions are deliberate design decisions.

***

## Documentation

- **Architecture & execution model**  
  → [README.architecture.md](README.architecture.md)

- **Design decisions & trade‑offs**  
  → [README.design.md](README.design.md)

- **Commercial usage & collaboration**  
  → [README.commercial.md](README.commercial.md)

- **About the author**  
  → [README.author.md](README.author.md)

***

## Status

*   Target framework: **.NET 8+**
*   API contract version: **v0.29 (frozen)**
*   Edition: **Free core**
*   Commercial edition: **WJb.Pro**

***

## Licensing

The **free core** of WJb is available under the **Apache License 2.0**.

Commercial, SaaS, and closed‑source usage requires a separate
commercial license available as **WJb.Pro**.

See [README.commercial.md](README.commercial.md) for details.

***

## Philosophy in One Sentence

> **WJb treats background execution as explicit domain code — predictable, traceable, and debuggable.**

***

## ❤️ Support

If this project helps you, you can support its development:  
👉 <https://ko-fi.com/ukrguru>
