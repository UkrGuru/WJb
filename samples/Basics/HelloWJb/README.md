# HelloWJb

A minimal console application demonstrating
how to run a background job using **WJb (Free edition)**.

This sample serves as a **hello‑world** entry point
and illustrates the explicit execution model of WJb.

---

## Requirements

- .NET 8 or higher
- WJb (Free edition)

---

## What this example does

1. Registers a single action (`PrintAction`)
2. Builds and starts a .NET host
3. Creates a job payload explicitly
4. Enqueues the job for background execution
5. Executes the job using WJb’s hosted processor

There is no implicit execution or hidden orchestration.

---

## How to run

```bash
dotnet run
```

Example output:

```text
info: WJb.JobProcessor[0] JobProcessor started
info: PrintAction[0] Hello WJb!
```

***

## Notes

*   Job execution starts only after the host is running
*   All execution is explicit and deterministic
*   Metadata is treated as configuration, not state
*   This example avoids persistence and retries by design

***