
🚀 WJb v0.27.0 — Release Notes
Release date: April 2026
Edition: Free (Apache 2.0)
Target runtime: .NET 8+


✅ Summary
Version v0.27.0 is a stability and contract-hardening release for WJb Free.
This release finalizes the public Free API, introduces a formally frozen API contract, and completes the architectural separation between Free and Commercial (Pro) capabilities.
Performance, queue semantics, and execution behavior are now explicitly defined and validated by executable benchmarks.


🔒 API Contract

✅ Introduced official frozen API contract for Free edition
✅ Public API documented in docs/api-v0.27.md
✅ All public interfaces reviewed and stabilized
✅ Any API not listed in the contract is considered internal
This establishes v0.27.x as a stable foundation for users and integrators.


✂️ Free vs Commercial Separation
The Free edition has been intentionally simplified and hardened:
Removed from Free API

❌ Runtime configuration reload
❌ Queue slot management (ReleaseSlot)
❌ Any IReloadable* interfaces
❌ Live scheduler or queue tuning
These capabilities are now explicitly reserved for the Commercial edition.
Free users receive a complete, self-contained execution model without operational or runtime control concerns.


⚙️ Queue Semantics (Now Contractual)
Queue behavior is now formally defined and verified:

✅ Strict priority ordering (higher priority always wins)
✅ FIFO ordering within the same priority
✅ Starvation of lower priorities is allowed by design
✅ No fairness guarantees implied or hidden
These rules are validated by queue semantics benchmarks, not assumptions.


🚄 Performance
v0.27.0 confirms WJb Free as a high-performance core library:

✅ ~700,000 jobs/sec (single-thread enqueue)
✅ ~2,000,000+ jobs/sec (multi-thread enqueue)
✅ Zero dependency on DI or infrastructure during enqueue
Benchmarks are intentionally minimal and focus on core behavior, not tuning.


🧠 Execution Model Clarified

✅ Jobs are executed at least once
✅ Ordering is guaranteed only within a single workflow chain
✅ No guarantees of global ordering or exact execution timing
✅ Actions must be idempotent
These guarantees are now explicitly documented and enforced.


🧩 Dependency Injection

✅ Simplified public registration via AddWJb(...)
✅ Internals fully hidden from public usage
✅ No manual wiring required for common scenarios
The public DI surface is now minimal, explicit, and stable.


📦 Target Framework

✅ Free edition: .NET 8 or higher
❗ Commercial edition will target .NET 10 only (not part of this release)
The Free edition remains aligned with .NET 8 LTS for broad compatibility.


🧪 Benchmarks as Contracts
Benchmarks in this release are treated as executable design contracts, not performance marketing:

Enqueue correctness
Multi-thread safety
Queue ordering semantics
Any change that breaks these benchmarks is considered a breaking change.


⚠️ Breaking Changes

Removal of runtime control APIs from Free edition
Removal of ReleaseSlot from Free queue abstractions
Users depending on these capabilities must migrate to the Commercial edition.


✅ Migration Notes
Most Free users do not need to change any code.
If you were:

manually releasing queue slots
relying on runtime reload hooks
those capabilities are no longer present in Free as of v0.27.0.


🏁 Conclusion
v0.27.0 finalizes WJb Free's core design.

Stable API
Explicit guarantees
High performance
Clear separation of concerns
Future releases can now build forward without revisiting fundamentals.


Thank you for using WJb.
