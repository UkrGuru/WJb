# WJb API — v0.29

This document describes the **frozen public API contract**
for WJb version **v0.29**.

Any breaking changes require a new minor or major API contract version.

---

## Target framework

- .NET 8+

---

## API contract status

- **Version:** v0.29
- **Status:** Frozen
- **Backward compatibility:** Guaranteed within v0.29

---

## Terminology

| Term      | Description |
|----------|-------------|
| Job       | A serialized payload representing a unit of work |
| Action    | A logical operation identified by a string code |
| Payload   | UTF‑8 encoded JSON string containing action code and metadata |
| Snapshot  | Immutable view of registered actions |

---

## Job payload format

A job payload is a UTF‑8 encoded JSON string
with the following stable structure:

```json
{
  "code": "action.code",
  "more": {
    "...": "..."
  }
}
```

### Fields

*   **`code`** — required, string identifier of the action
*   **`more`** — optional structured metadata object

The payload schema is part of the frozen **v0.29** contract  
and must not change within this version.

***

## Job serialization

### `CompactAsync`

```csharp
Task<string> CompactAsync(
    string actionCode,
    object? jobMore = null,
    CancellationToken stoppingToken = default)
```

#### Description

Compacts an action code and associated metadata
into a serialized job payload.

#### Behavior

*   Produces a JSON string matching the job payload format
*   Does not enqueue or execute the job
*   Does not mutate internal state
*   Does not validate action existence
*   Does not perform scheduling or routing

***

## Job enqueuing

### `EnqueueJobAsync`

```csharp
Task EnqueueJobAsync(
    string job,
    Priority priority = Priority.Normal,
    CancellationToken stoppingToken = default)
```

#### Description

Enqueues a serialized job payload into the processing queue.

#### Notes

*   The method is asynchronous and non‑blocking
*   No delivery or execution guarantees are implied
*   Ordering depends on the underlying queue implementation
*   The job payload is treated as an opaque string

***

## Priority

```csharp
public enum Priority
{
    ASAP,   // highest
    High,
    Normal,
    Low     // lowest
}
```

### Semantics

*   Priority affects **queue ordering only**
*   Priority does not imply retry behavior
*   Priority does not imply execution guarantees

***

## Cron metadata

Actions may declare scheduling metadata using the `more` object:

```json
{
  "code": "cleanup.temp",
  "more": {
    "cron": "0 5 * * *"
  }
}
```

### Notes

*   Cron expressions are strings and are not validated during serialization
*   Absence of the `cron` field means the action is non‑scheduled
*   Cron indexing is derived from immutable snapshots
*   Scheduling metadata is treated as configuration, not control flow

***

## Snapshots

Snapshots represent an immutable view
of registered actions at a single point in time.

### Properties

*   Thread‑safe for concurrent readers
*   Built deterministically
*   Replaced atomically
*   Never partially visible

Snapshots may be rebuilt
without affecting readers observing previous versions.

***

## Execution semantics

*   Job execution is explicit and deterministic
*   Actions control their own execution lifecycle
*   Routing and continuation are action‑owned
*   Infrastructure executes but never orchestrates

No implicit retries, pipelines, or orchestration layers exist
within this API contract.

***

## Threading and safety

*   No public API requires external synchronization
*   Reference replacement is used instead of locks
*   Callers must assume eventual consistency between snapshots

***

## Non‑goals of this API

The **v0.29** API intentionally does **not**:

*   Guarantee job delivery
*   Implement retries or backoff policies
*   Persist jobs or state by default
*   Manage worker lifetimes or scaling
*   Perform implicit background execution
*   Provide workflow orchestration primitives

These concerns are delegated to
hosting infrastructure and domain code.

***

## Compatibility guarantees

Within the **v0.29** API contract:

*   Method signatures will not change
*   Payload format will not change
*   Execution semantics will not be redefined

Future versions may extend functionality,
but any breaking change will introduce
a new API contract version.

***