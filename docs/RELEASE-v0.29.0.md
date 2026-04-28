# WJb v0.29 Release Notes

**Release date:** 2026‑04‑27  
**API contract:** v0.29 (frozen)

---

## Overview

Version **v0.29** finalizes the Free edition of WJb
and establishes a stable, long‑term public API contract.

This release focuses on **contract clarity**, **deterministic behavior**,
and **documentation alignment** rather than adding new features.

It is intended to be a solid baseline for production use
and future extension through compatible API layers.

---

## Highlights

- Frozen public API contract (v0.29)
- Explicit, deterministic execution model
- Finalized workflow action lifecycle
- Stable metadata and job payload format
- Fully aligned and consolidated documentation

---

## API Stability

- The **public API is frozen** under contract version **v0.29**
- No breaking changes are expected within this contract
- Payload format and execution semantics are guaranteed stable

Any future breaking changes will introduce a new API contract version.

---

## Execution Model

- Job execution remains fully explicit
- Actions own their execution lifecycle
- Routing and continuation logic is action‑owned
- Infrastructure components execute but never orchestrate

No implicit retries, pipelines, or background automation
are introduced in this release.

---

## Metadata and Scheduling

- Jobs use a stable, UTF‑8 encoded JSON payload
- Metadata is treated as configuration, not state
- Optional cron scheduling is expressed via action metadata
- Scheduling behavior is deterministic and snapshot‑driven

---

## Documentation

All documentation has been reviewed and aligned
with the actual implementation:

- `README.md` — project overview and philosophy
- `README.architecture.md` — system‑level architecture
- `README.design.md` — engineering decisions and trade‑offs
- `README.author.md` — author perspective
- `README.commercial.md` — collaboration and support
- `README.licensing.md` — licensing model
- `api-v0.29.0.md` — frozen public API contract

---

## Non‑Goals (Confirmed)

The v0.29 release intentionally does **not** include:

- Delivery guarantees
- Automatic retries or backoff policies
- Persistent storage by default
- Distributed coordination or orchestration
- Implicit background execution behavior

These concerns remain outside the scope of the Free edition
and are delegated to hosting infrastructure or domain code.

---

## Compatibility

- Requires **.NET 8+**
- Backward compatible within the v0.29 contract
- Safe to adopt for long‑lived systems

---

## Status

✅ **WJb Free Edition — Stable**  
✅ **API v0.29 — Frozen**  
✅ **Documentation — Complete**  

This release marks the completion of the Free edition baseline.