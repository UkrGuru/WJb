# Fibonacci Value-Range Workflow Sample

This sample demonstrates a **single, concrete WorkflowWJb scenario**:

> Generate all Fibonacci numbers whose **values** are within a given range `[from..to]`.

This README documents **only this sample**. It is intentionally limited in scope and does not describe WorkflowWJb in general.

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

1. `FibonacciStartAction`
2. `FibonacciBuildAction`

Only the **first job** is enqueued manually.

---

## Workflow structure

```text
fib-start  -->  fib-build
```

- `fib-start` prepares the Fibonacci state
- `fib-build` generates all result values

No branching, no conditions, no alternative paths.

---

## FibonacciStartAction

Purpose:

- Read `from` / `to` (value bounds)
- Walk the Fibonacci sequence forward
- Stop at the **first Fibonacci number where `a >= from`**
- Pass this state to the next action

Core invariant:

```text
a is the first Fibonacci value >= from
```

Relevant logic:

```csharp
long a = 0;
long b = 1;

while (a < from)
{
    var next = a + b;
    a = b;
    b = next;
}
```

After this loop:

```text
a >= from
(a, b) is a valid consecutive Fibonacci pair
```

This ensures the build step starts at the **correct value**, not at index zero.

---

## FibonacciBuildAction

Purpose:

- Generate Fibonacci numbers **by value**
- Emit each value starting from `a`
- Stop when the value exceeds `to`

Core loop:

```csharp
while (a <= to)
{
    values.Add(a);
    var next = a + b;
    a = b;
    b = next;
}
```

This guarantees:

- No index-based interpretation
- No overflow caused by large indices
- Deterministic, bounded execution

---

## Execution

The job is started like this:

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
info: FibonacciStartAction[0] Start Fibonacci values in range [10..100]: a=13, b=21
info: FibonacciBuildAction[0] Fibonacci values [10..100] = [13,21,34,55,89]```
```

---

