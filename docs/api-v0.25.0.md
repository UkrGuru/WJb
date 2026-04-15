# 🏷️ WJb v0.25.0

**Release type:** Stabilization & API clarification  
**Target framework:** .NET 10+

This release formalizes the **core WJb execution model** and documents the **stable public API**.  
`v0.25.0` is the recommended baseline for production use.

***

## ✅ What’s stable in v0.25.0

### 🔒 Public API contract

*   Public API is now explicitly defined and documented
*   Undocumented APIs are considered internal and unsupported
*   New API contract document added:
    *   `docs/api-v0.25.0.md`

### 🧱 Declarative workflow model

*   Workflow progression is driven by `ActionItem.More` metadata
*   Built‑in support for:
    *   `next` — workflow chaining
    *   `cron` — scheduling
    *   `delay` — deferred execution
*   No imperative job chaining inside actions

### ⚡ Explicit execution semantics

*   Actions implement **only** `ExecAsync`
*   At‑least‑once execution model
*   Ordering is guaranteed **only within a single workflow chain**
*   Parallelism depends on the queue implementation

***

## ✂️ Simplifications & clarifications

*   Removed implied or legacy action‑chaining patterns
*   Clarified that actions **must not enqueue jobs directly**
*   Reinforced idempotency requirement for actions
*   Explicitly documented **non‑goals** (no orchestration, no workflow engine)

***

## 🧠 Design philosophy (unchanged)

WJb remains intentionally small and disciplined:

*   ✅ Predictable behavior over magic
*   ✅ Declarative flow over imperative orchestration
*   ✅ Minimal abstractions
*   ❌ No retries, sagas, or business rules engine
*   ❌ No hidden scheduling guarantees

If it’s not documented — it’s not part of the contract.

***

## 📦 Migration notes

*   Existing users: **no functional changes** if you already rely on `AddWJb(...)` and `ActionItem`
*   If you previously chained jobs inside actions, migrate to metadata‑driven workflows (`More.next`)

***

## 🔭 What’s next

*   Minor iterations within `0.x` will remain intentional and documented
*   `v1.0` will be reserved for long‑term API stability — no date announced

***

Thank you for using **WJb** ❤️  
Structured background jobs, without framework gravity.

***
