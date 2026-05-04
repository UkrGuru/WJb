# Design Decisions and Trade‑offs

This document describes **local engineering decisions**
made within the WJb architecture.

It explains **how specific mechanisms are implemented**,
**which alternatives were considered**,
and **what trade‑offs were explicitly accepted**.

System‑level goals and structural overview
are described in `README.architecture.md`.

***

## Explicit execution flow

Job execution in WJb is fully explicit.

There are:

*   no implicit retries
*   no background pipelines
*   no hidden orchestration layers
*   no automatic job transitions

All execution steps are initiated through
explicit enqueue operations
and action‑owned routing logic.

Execution behavior is therefore always visible
either in code or in explicitly defined job metadata.

This improves reasoning during debugging,
testing, and incident analysis.

***

## Immutable snapshots

Runtime configuration in WJb is modeled as immutable snapshots.

Readers always observe a consistent view of action definitions,
and execution logic never mutates shared configuration in place.

Snapshots are:

*   built deterministically
*   replaced atomically
*   never partially visible

Each execution step operates against a single snapshot,
avoiding concurrency hazards and hidden state transitions.

***

## Queue‑first model

All work enters the system through explicit queue operations.

Execution is never triggered directly by method calls.
This enforces a strict separation between:

*   **expressing intent** (enqueue)
*   **performing execution** (dequeue and process)

As a result, execution timing, ordering, and side effects
remain observable and externally controllable.

***

## Avoidance of runtime magic

WJb intentionally avoids:

*   implicit orchestration
*   hidden execution conventions
*   background lifecycle automation
*   framework‑driven behavior assumptions

Runtime behavior is driven by:

*   explicit code
*   declared metadata
*   deterministic execution paths

This reduces hidden coupling
and keeps behavior locally understandable and reproducible.

***

## Accepted trade‑offs

These design decisions intentionally result in:

*   more explicit setup compared to opinionated frameworks
*   fewer convenience abstractions
*   higher upfront integration effort

These trade‑offs are accepted in exchange for:

*   long‑term API stability
*   predictable execution behavior
*   reduced operational ambiguity
*   ease of reasoning in production environments

***
