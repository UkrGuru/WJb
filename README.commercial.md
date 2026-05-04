# Commercial Edition

The Commercial Edition of **WJb** extends the Base execution core with additional
capabilities intended for **production**, **commercial**, **SaaS**, and
**closed‑source** environments.

It builds on the same explicit execution model and stable API contract,
while providing runtime features, integrations, and guarantees
required for long‑term operation in real‑world systems.

## Base vs Commercial Comparison

| Area                          | **WJb Base**                      | **WJb Commercial (WJb.Pro)**                      |
| ----------------------------- | --------------------------------- | ------------------------------------------------- |
| Product name                  | WJb                               | WJb                                               |
| Package                       | `WJb`                             | `WJb.Pro`                                         |
| License                       | Apache License 2.0                | WJb — Commercial Capability License (Solo / Team) |
| Intended use                  | Open‑source, learning, evaluation | Commercial, SaaS, production, closed‑source       |
| Job execution                 | ✅ Explicit, deterministic         | ✅ Explicit, deterministic                         |
| Job payloads                  | ✅ JSON, explicit                  | ✅ JSON, explicit                                  |
| Execution model               | ✅ Queue‑first                     | ✅ Queue‑first                                     |
| Action model                  | ✅ `IAction`, explicit routing     | ✅ Same, extended capabilities                     |
| Workflow routing              | ✅ Explicit (`IWorkflowAction`)    | ✅ Same, plus extensions                           |
| Scheduling (cron)             | ✅ Basic                           | ✅ Extended                                        |
| Settings registry             | ❌ Not available                   | ✅ Available                                       |
| Runtime reload                | ❌ Not supported                   | ✅ Supported                                       |
| Persistence                   | ❌ In‑memory only                  | ✅ Optional implementations                        |
| Delivery guarantees           | ❌ None                            | ✅ Optional / configurable                         |
| Advanced orchestration        | ❌ Not provided                    | ✅ Available                                       |
| Production hardening features | ❌ Minimal                         | ✅ Included                                        |
| Author support                | ❌ Community only                  | ✅ Direct author support                           |
| Commercial redistribution     | ❌ Not permitted                   | ✅ Permitted under license                         |

***

### Notes

*   **WJb Base** is intentionally minimal and deterministic.  
    If a feature is not listed, it does not exist.
*   **WJb.Pro** does not change the execution philosophy — it **extends** it.
*   Both editions share the same core concepts and API shape.
*   Migration from Base to Commercial does **not** require rewriting domain logic.

***

## License Types

The commercial capabilities of **WJb** are available under the  
**WJb — Commercial Capability License**, offered in the following variants:

*   **Solo License** — for individual developers  
    <https://ukrguru.gumroad.com/l/wjb-solo-lic>

*   **Team License** — for companies and teams  
    <https://ukrguru.gumroad.com/l/wjb-team-lic>

***

## WJb.Pro

**WJb.Pro** is the commercial **package** of the **WJb product**.

It enables commercial capabilities of **WJb** and is intended for
**production**, **commercial**, **SaaS**, and **closed‑source** environments.

Use of WJb.Pro is governed by the  
**WJb — Commercial Capability License (Solo)**.

***

## Collaboration and Support

**WJb** is actively developed and maintained by its author,
with direct support available for commercial users.

You can contact me regarding:

*   commercial licensing and renewals
*   migration from WJb Base to commercial capabilities
*   architectural adaptation to existing systems
*   custom extensions and integrations
*   consulting and long‑term technical support

📧 **<ukrguru@gmail.com>**

***