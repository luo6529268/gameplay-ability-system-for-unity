# NTSD28-B6-HELD-RECIPROCAL-FAILURE-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-HELD-RECIPROCAL-FAILURE-AUDIT-001
status: SUPERSEDED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::settle_held_refill_objects invalid negative reciprocal handling; EXE B1E13AE1, closure 39DDDA15.
evidence: Authority reports invalid parent/mismatch without relation mutation; Unity HeldObjectProcessAll clears child LinkState to zero; same-state rule difference is confirmed but normal lifecycle reachability remains pending; terminal/cover2 current and release counts are zero and current referenced weapon actions contain no state12/18; no code/content/Scene changes.
-->

> 状态：`SUPERSEDED / REACHABILITY_RESOLVED_BY_NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001`

Invalid negative reciprocal入场时，Authority只报告失败并保留关系；Unity会清零child LinkState。
同状态行为差异已确认，但正常holder removal/slot reuse是否产生该状态尚未闭合，所以先进入
`NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001`，不直接写actual。

完整边界见`docs/ai/TASKS/NTSD28-B6-HELD-RECIPROCAL-FAILURE-AUDIT-001.md`；本轮无脚本、
content或Scene改动。

后续`NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001`已证明Authority
despawn-before-release清理与Unity release/reuse缺口，取代本记录的reachability pending状态。
