# Design decisions and trade‑offs

This document focuses on **local engineering decisions**
made within the overall WJb architecture.

It explains *how* specific mechanisms are implemented,
*what alternatives were considered*,
and *which trade‑offs were explicitly accepted*.

Architectural goals and system‑level structure
are described in README.architecture.md.

---

## Explicit execution flow

Job execution in WJb is entirely explicit.

There are no implicit retries, background pipelines,
or hidden orchestration layers.
All execution transitions are visible
either in code or in explicitly defined job metadata.

This approach makes execution behavior easier to reason about
during debugging, testing, and incident analysis.

---

## Immutable snapshots

State in WJb is modeled as immutable snapshots.

Readers always observe a consistent system view,
and execution logic never mutates shared or persisted state in place.

Each execution step operates on its own snapshot,
producing an explicit state transition.
This design reduces concurrency‑related complexity
and improves predictability under load.

---

## Queue‑first model

All work enters the system through explicit enqueue operations.

There is no execution initiated directly by method calls.
This enforces a strict boundary between:

- expressing intent
- performing execution

By design, execution timing, ordering, and side effects
remain observable and externally controllable.

---

## Avoidance of runtime magic

WJb intentionally avoids:

- reflection‑driven dispatch
- implicit dependency injection behavior
- implicitly started background threads
- framework‑level execution conventions

Instead, all runtime behavior is code‑driven,
locally observable, and reproducible.

This reduces hidden coupling
and lowers the cognitive load for maintainers.

---

## Accepted trade‑offs

These design decisions intentionally result in:

- more explicit configuration compared to opinionated frameworks
- less implicit automation
- slightly higher upfront integration effort

These trade‑offs are considered acceptable
in exchange for long‑term stability,
operational predictability,
and ease of reasoning in production systems.
