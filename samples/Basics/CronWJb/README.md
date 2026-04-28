# CronWJb

A minimal example demonstrating
**cron-based scheduled job execution**
using **WJb (Free edition)**.

This sample shows how scheduling metadata
can be attached to actions explicitly.

---

## Overview

CronWJb registers actions with associated cron expressions
and executes them deterministically using WJb’s scheduler.

There is no implicit orchestration or hidden background logic.

---

## What this example demonstrates

1. A .NET host is started
2. Actions are registered with cron metadata
3. The WJb scheduler is enabled explicitly
4. Jobs are generated according to cron expressions
5. Actions execute on schedule

---

## Cron metadata

Cron expressions are attached to actions via metadata:

```csharp
new JsonObject
{
    ["cron"] = "* * * * *",
    ["priority"] = "ASAP",
    ["message"] = "Minute tick ✅"
}
```

*   Cron expressions are treated as configuration
*   They are not validated by WJb
*   Absence of a cron expression means non‑scheduled action

***

## Action implementation

```csharp
public sealed class DummyAction(ILogger<DummyAction> logger) : IAction
{
    public Task ExecAsync(JsonObject? jobMore, CancellationToken _)
    {
        var message = jobMore.GetString("message") ?? "<empty>";
        logger.LogInformation(message);
        return Task.CompletedTask;
    }
}
```

***

## Run

```bash
dotnet run
```

Example output:

```text
12:00:00 CronWJb started. Waiting for cron ticks...
12:00:00  - HelloEveryMinute: * * * * *
12:01:00 Minute tick ✅
```

***

## Notes

*   Scheduling is deterministic and snapshot‑based
*   No persistence or retries are involved
*   JSON‑based configuration is intentionally not shown here

Advanced configuration examples are available
in `samples/Advanced`.

***
