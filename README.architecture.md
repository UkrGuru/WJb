# Architecture overview

This document provides an architectural overview of WJb
and explains the core structural decisions behind the system.

The focus is not on feature completeness,
but on **behavioral clarity**, **operational predictability**,
and **long‑term maintainability**.

---

## Architectural goals

WJb was designed with the following high‑level goals:

- deterministic and observable execution behavior
- clear separation between intent and execution
- minimal hidden state and implicit control flow
- stable contracts suitable for long‑living systems
- ease of reasoning under production conditions

These goals intentionally prioritize predictability
over flexibility and automation.

---

## High‑level system model

At a high level, WJb models background processing as
a sequence of explicit state transitions applied to immutable data.

The core concepts are:

- an explicit job payload describing intent
- a queue as the only entry point for execution
- immutable snapshots representing system state
- a deterministic execution loop applying actions to payloads

---

## Queue‑first architecture

All work in WJb enters the system through explicit enqueue operations.

Method calls never result in immediate execution.
Instead, intent is captured as a payload and persisted to a queue,
establishing a clear boundary between:

- requesting work
- executing work

This architectural boundary makes execution timing,
ordering, and side effects fully observable and controllable.

---

## Explicit execution lifecycle

Job execution follows a fully explicit lifecycle.

There are no implicit retries, no background pipelines,
and no hidden execution chains.

Each stage of execution is:

- visible in code
- traceable via logs and state
- reproducible during debugging

If a job is executed, it is because it was explicitly enqueued.
If execution continues, it is because an action explicitly decided so.

---

## Immutable state and snapshots

System state in WJb is represented as immutable snapshots.

Execution logic never mutates shared or persisted state in place.
Instead, each execution step operates on its own snapshot,
producing a new state representation that can be safely observed
by concurrent readers.

This model greatly simplifies reasoning about behavior
under parallel or delayed execution.

---

## Deterministic scheduling model

Scheduling in WJb is deterministic and data‑driven.

Scheduling is treated as execution metadata,
not as a separate control system.

Optional scheduling metadata (such as cron expressions)
is attached directly to actions,
not embedded in global configuration or background services.

---

## Separation of concerns

WJb maintains strict separation between:

- job definition
- scheduling metadata
- execution logic
- infrastructure concerns (queues, storage, logging)

Infrastructure components are responsible for execution only.
All orchestration and routing decisions remain
in explicit domain code.

---

## Avoidance of implicit infrastructure behavior

The architecture intentionally avoids:

- reflection‑driven dispatch
- implicit dependency injection behavior
- implicitly started background threads
- framework‑level execution magic

All execution paths are explicit,
locally visible, and code‑driven.

---

## Evolution and backward compatibility

WJb uses a contract‑based versioning strategy.

Once an API contract is declared stable,
it is treated as immutable.

Extensions may be layered on top of existing contracts,
but previously released behavior is preserved.

This approach enables long‑term system evolution
without breaking existing deployments.

---

## Architectural trade‑offs

These architectural choices intentionally trade:

- convenience for explicitness
- automation for predictability
- abstraction depth for debuggability

These trade‑offs are considered acceptable
for systems where correctness and operational clarity
matter more than rapid feature experimentation.

---

## Intended architectural audience

This architecture is intended for engineers who:

- design and maintain infrastructure code
- operate systems in production
- value explicit behavior over framework automation
- expect systems to evolve over long timelines
