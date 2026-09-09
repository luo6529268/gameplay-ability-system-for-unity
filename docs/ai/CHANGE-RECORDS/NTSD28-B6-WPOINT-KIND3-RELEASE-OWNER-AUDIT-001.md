# NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-AUDIT-001
status: SUPERSEDED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::settle_held_refill_objects WPoint kind-3 tail; EXE B1E13AE1, closure 39DDDA15.
evidence: BattleHeldObjectWriter.DropRandomly selected as sole actual writer; fixed four-draw 6/7/4/5 sequence and authored-per-axis fallback frozen; focused/SelfCheck/Play responsibilities defined; no code/content/Scene changes.
-->

> 状态：`SUPERSEDED / CORRECTED_BY_NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001`

WPoint kind-3 release 只需在 `BattleHeldObjectWriter.DropRandomly()` 处修正：接收 formal
WPoint，无条件draw frame/X/Y/Z四个 synchronized值，每轴 authored `dv* != 0` 时使用
authored，否则用 random，并移除 Z 的 `* 0.2`。既有 release tick/link cleanup 保持。

完整 owner、test matrix、不变量与回滚见
`docs/ai/TASKS/NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-AUDIT-001.md`。

后续 production 候选为 `NTSD28-B6-WPOINT-KIND3-RELEASE-PRODUCTION-001`；它排在
`NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001`之后。由于前置 CPoint throw
package 仍未通过 Unity runtime，本轮不叠加新行为修改。

## 后续纠正（2026-09-08）

本记录保留RNG与final-write历史事实，但其sole-owner结论遗漏`RunStep12()`在real/generic
DVX release后的提前return。由
`NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001`正式纠正并取代实施边界。
