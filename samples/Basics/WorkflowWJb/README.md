# Fibonacci Value‑Range Workflow Sample

This sample demonstrates a **single, concrete WorkflowWJb scenario**:

> Generate all Fibonacci numbers whose **values**
> fall within a given range `[from..to]`.

This README documents **only this sample**.
It deliberately avoids describing WorkflowWJb in general.

---

## What this sample does

Input parameters:

```text
from = 10
to   = 100
```

Result produced by the workflow:

```text
[13, 21, 34, 55, 89]
```

The workflow consists of **two actions**:

1.  `FibonacciStartAction`
2.  `FibonacciBuildAction`

Only the **first job** is enqueued manually.

***

## Workflow structure

```text
fib-start  -->  fib-build
```

*   `fib-start` prepares the Fibonacci state
*   `fib-build` generates the result values

There are no branches or alternative paths.

***

## FibonacciStartAction

Purpose:

*   Read `from` / `to` (value bounds)
*   Walk the Fibonacci sequence forward
*   Stop at the **first value where `a >= from`**
*   Pass this state to the next action

Core invariant:

```text
a is the first Fibonacci value >= from
```

***

## FibonacciBuildAction

Purpose:

*   Generate Fibonacci numbers **by value**
*   Emit values starting from `a`
*   Stop once the value exceeds `to`

This ensures deterministic,
bounded, and index‑free execution.

***

## Execution

The workflow is started explicitly:

```csharp
var job = await jobs.CompactAsync(
    "fib-start",
    new { from = 10, to = 100 }
);

await jobs.EnqueueJobAsync(job);
```

Expected log output:

```text
info: WJb.JobProcessor[0] JobProcessor started
info: FibonacciStartAction[0] Start Fibonacci range [10..100]: a=13, b=21
info: FibonacciBuildAction[0] Fibonacci values [10..100] = [13,21,34,55,89]
```

***

## Notes

*   Routing is explicit and action‑owned
*   No orchestration exists outside actions
*   Metadata is treated as configuration
*   This sample intentionally avoids persistence and retries

***
