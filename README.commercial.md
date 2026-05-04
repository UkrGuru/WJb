# WJb — Commercial Licensing

## Overview

This document defines the licensing terms and commercial usage rules
for **WJb** and its commercial capabilities.

The **Base edition** of WJb is intentionally minimal and remains free for
non‑commercial use, evaluation, and open‑source projects.

Use of WJb in **commercial**, **SaaS**, **production**, or
**closed‑source** environments requires a commercial license.

***

## Commercial Edition

The **Commercial Edition of WJb** extends the Base execution core with
additional capabilities required for commercial, SaaS, and production use.

It preserves the same explicit execution model and stable API contract,
while adding runtime features, integrations, and operational guarantees
needed for long‑term real‑world systems.

***

## Base vs Commercial

| Area                          | **WJb Base**                      | **WJb Commercial (WJb.Pro)**                      |
| ----------------------------- | --------------------------------- | ------------------------------------------------- |
| Package                       | `WJb`                             | `WJb.Pro`                                         |
| License                       | Apache License 2.0                | WJb — Commercial Capability License               |
| Intended use                  | Open‑source, evaluation           | Commercial, SaaS, production, closed‑source       |
| Job execution                 | ✅ Explicit, deterministic         | ✅ Explicit, deterministic                         |
| Workflow routing              | ✅ Explicit                        | ✅ Extended                                        |
| Scheduling (cron)             | ✅ Basic                           | ✅ Extended                                        |
| Persistence                   | ❌ In‑memory only                  | ✅ Optional implementations                        |
| Runtime reload                | ❌ Not supported                   | ✅ Supported                                       |
| Delivery guarantees           | ❌ None                            | ✅ Optional / configurable                         |
| Production hardening          | ❌ Minimal                         | ✅ Included                                        |
| Author support                | ❌ Community only                  | ✅ Direct support                                  |

### Notes

* **WJb Base** is intentionally minimal and deterministic.
  If a feature is not listed, it does not exist.
* **WJb.Pro** extends the Base edition without changing its execution philosophy.
* Both editions share the same core API and concepts.
* Migration from Base to Commercial does **not** require rewriting domain logic.

***

## License Types

Commercial use of **WJb** is governed by the  
**WJb — Commercial Capability License**, available in the following variants:

* **Solo License** — for individual developers using WJb in commercial projects  
  → https://ukrguru.gumroad.com/l/wjb-solo-lic

* **Team License** — for companies and development teams  
  → https://ukrguru.gumroad.com/l/wjb-team-lic

***

## WJb.Pro Package

**WJb.Pro** is the commercial package of the **WJb product**.

It enables commercial capabilities and is intended for
production, SaaS, and closed‑source environments.

Use of WJb.Pro requires the appropriate
**WJb — Commercial Capability License**.

***

## Commercial Usage Summary

A commercial license is required if WJb is used in:

* paid products or services
* SaaS or hosted platforms
* internal company systems
* closed‑source or proprietary software
* redistributed commercial software

The Base edition remains suitable for
open‑source projects, learning, and evaluation.

***

## Collaboration and Support

WJb is developed and maintained by its author.

Commercial users have access to direct collaboration and support,
including:

* licensing and renewals
* migration from Base to Commercial
* architectural guidance
* custom extensions and integrations
* long‑term technical support

📧 <ukrguru@gmail.com>
