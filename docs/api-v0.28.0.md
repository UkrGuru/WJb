# WJb API — v0.28

This document describes the **frozen public API contract** for WJb version **v0.28**.

Any breaking changes will require a new major or minor contract version.

---

## Target Framework

- .NET 8+

---

## API Contract Status

- **Version:** v0.28
- **Status:** Frozen
- **Backward compatibility:** Guaranteed within v0.28

---

## Terminology

| Term      | Description |
|----------|-------------|
| Job       | A serialized payload representing a unit of work |
| Action    | A logical operation identified by a string code |
| Payload   | JSON string containing action code and metadata |
| Snapshot  | Immutable view of registered actions |

---

## Job Payload Format

A job payload is a UTF‑8 encoded JSON string with the following structure:

```json
{
  "code": "action.code",
  "more": {
    "...": "..."
  }
}
````

### Fields

*   `code` — **required**, string identifier of the action
*   `more` — optional structured metadata object

The payload format is stable and must not be changed in v0.28.

***

## Job Serialization

### CompactAsync

```csharp
Task<string> CompactAsync(
    string actionCode,
    object? jobMore = null,
    CancellationToken stoppingToken = default)
```

#### Description

Compacts an action code and associated metadata into a job payload.

#### Behavior

*   Produces a JSON string matching the job payload format
*   Does not enqueue or execute the job
*   Does not mutate internal state
*   Does not validate action existence

***

## Job Enqueuing

### EnqueueJobAsync

```csharp
Task EnqueueJobAsync(
    string job,
    Priority priority = Priority.Normal,
    CancellationToken stoppingToken = default)
```

#### Description

Enqueues a job payload into the processing queue.

#### Notes

*   The method is asynchronous and non-blocking
*   No execution or delivery guarantees are implied
*   Ordering depends on the underlying queue implementation
*   The job payload is treated as an opaque string

***

## Priority

```csharp
public enum Priority
{
    ASAP, // highest
    High,
    Normal,
    Low // lowest
}
```

### Semantics

*   Priority affects queue ordering only
*   Priority does not imply retry behavior or execution guarantees

***

## Cron Metadata

Actions may declare cron scheduling metadata using the `more` object:

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
*   Absence of the `cron` field means the action is non-scheduled
*   Cron indexing is derived from immutable snapshots

***

## Snapshots

Snapshots represent an immutable view of registered actions at a point in time.

### Properties

*   Thread-safe for concurrent readers
*   Built deterministically
*   Replaced atomically
*   Never partially visible

Snapshots may be rebuilt without affecting readers observing previous versions.

***

## Threading and Safety

*   No public API requires external synchronization
*   Reference swaps are used instead of locks
*   Callers must assume eventual consistency between snapshots

***

## Non-Goals of This API

The v0.28 API intentionally does **not**:

*   Guarantee job delivery
*   Implement retries
*   Persist jobs or state by default
*   Manage worker lifetimes or scaling
*   Perform implicit background execution

These concerns are delegated to the hosting infrastructure.

***

## Compatibility Guarantees

Within the v0.28 contract:

*   Method signatures will not change
*   Payload format will not change
*   Existing semantics will not be redefined

Future versions may extend functionality but will introduce a new contract
if breaking changes are required.

***