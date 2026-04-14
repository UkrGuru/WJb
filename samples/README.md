# WJb Samples

This directory contains a small, focused set of samples demonstrating
**basic usage patterns of the WJb API**.

The goal of these samples is to help you get started quickly and understand
the core execution model of WJb.

> These are **learning examples**, not production-ready solutions.

---

## ✅ Included Samples

### Basics
- **HwlloWJb**  
  Minimal console application showing the first job execution.

- **QueueWJb**  
  Basic priority queue usage.

- **TimerWJb**  
  Delayed job execution using timers.

- **WorkflowWJb**  
  Job chaining using `NextAsync`, where one job explicitly schedules the next.

---

## Scope

These samples focus strictly on demonstrating the WJb API
and its execution model.

Production concerns such as persistence, retries, metrics,
UI integration, security, and full system architectures
are intentionally out of scope and are not shown here.

## 🎯 Philosophy

WJb samples focus on:
- clarity over completeness
- explicit behavior over magic
- API understanding over real-world infrastructure

This keeps the library lightweight and avoids misleading examples.

---

## 📦 NuGet

- Package: `WJb`
- Namespace: `WJb`

https://www.nuget.org/packages/WJb/

---

## 📄 Licensing

Samples follow the same dual-license model as WJb itself.
See the main repository README for details.