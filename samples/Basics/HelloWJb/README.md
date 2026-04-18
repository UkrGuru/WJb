# HelloWJb

A minimal console application demonstrating how to run a background job
using **WJb (Free edition)**.

This project is intentionally simple and serves as a **hello-world**
entry point for WJb.

---

## Requirements

- .NET 8 or higher
- WJb (Free edition)

---

## What this example does

1. Registers a single action (`PrintAction`)
2. Creates a background job with a small payload
3. Enqueues the job on application startup
4. Executes the job using WJb’s hosted worker

---

## How to run

```bash
dotnet run
``

Example output order:

```text
info: WJb.JobProcessor[0] JobProcessor started
info: PrintAction[0] Hello WJb!
```

***