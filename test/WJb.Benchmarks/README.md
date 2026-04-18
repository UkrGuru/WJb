# WJb – Core Guarantees and Engineering Contracts

WJb is a high‑performance job enqueue core with **strictly defined, executable guarantees**.
All claims about performance and behavior are backed by reproducible benchmarks and tests.

This repository follows a **proof‑first engineering model**:
no feature is accepted without measurable justification.

---

## Core Guarantees (Verified)

### Ingest / Enqueue Performance (Core, No DI)

Measured on modern desktop hardware (Release, x64, core‑only).

- **Single‑thread ingest**: **~500k+ jobs/sec**
- **Multi‑thread ingest (shared queue, ~20 threads)**: **~2M+ jobs/sec**

These numbers are enforced by executable benchmarks.
Any regression **without measurable new semantics** is rejected.

---

### Queue Semantics (Strict by Design)

The WJb core queue follows **strict priority semantics**.

Guarantees:
- ✅ **Strict priority ordering**
- ✅ **FIFO within the same priority**

Non‑guarantees (by design):
- ❌ No fairness guarantees
- ❌ Lower priorities **may starve**
- ❌ No priority aging

These semantics are **intentional** and **verified by executable contracts**.

---

## Engineering Principles (Non‑Negotiable)

### 0. Baseline Rule (Fixed Forever)

**No change is accepted if:**
- ❌ there is no reproducible benchmark
- ❌ there is no previous baseline
- ❌ there is no proof of:
  - either performance improvement
  - or *measurable* new semantics
- ❌ there is no test protecting the change

If a change cannot be proven — it is rejected.

---

## What We Benchmark (Separately)

Different costs must never be mixed in a single benchmark.

### ❌ What must NOT be mixed
- enqueue
- job lifecycle
- processing
- fairness
- backpressure
- cancellation
- hosted services

These are different cost domains.

---

### ✅ Benchmark Classes

WJb defines **four independent benchmark classes**:

### 1. Benchmark #1 — Ingest / Enqueue Only (Critical)

**Purpose**
- Protect core enqueue performance.

**Contract**
- ❌ No JobProcessor execution
- ❌ No BackgroundService
- ❌ No processing
- ✅ Only: Compact + Enqueue

**Metrics**
- jobs/sec
- allocations/job
- GC pressure

Any regression here **must be justified or reverted**.

---

### 2. Benchmark #2 — Queue Semantics

**Purpose**
- Prove that queue behavior is real and justified.

**Scenarios**
- FIFO within priority
- Strict priority precedence

**Explicitly NOT tested**
- Fairness
- Starvation prevention

These semantics are verified by deterministic, executable contracts.
If a behavior cannot be proven — it does not exist.

---

### 3. Benchmark #3 — Processing Throughput (Runtime)

**Purpose**
- Measure the real cost of the runtime pipeline.

**Contract**
- ✅ JobProcessor enabled
- ✅ Minimal action execution
- ✅ Variable parallelism

**Rule**
- ❌ Must NEVER be compared to ingest benchmarks
- ✅ Only compared between runtime implementations

---

### 4. Benchmark #4 — End‑to‑End (Integration)

**Purpose**
- Demonstration and documentation.

Includes:
- enqueue → processing → completion
- logging
- cancellation
- shutdown behavior

This benchmark is **documentation‑oriented**, not optimization‑oriented.

---

## Repository Layout

Benchmarks are part of the repository and versioned with the code.