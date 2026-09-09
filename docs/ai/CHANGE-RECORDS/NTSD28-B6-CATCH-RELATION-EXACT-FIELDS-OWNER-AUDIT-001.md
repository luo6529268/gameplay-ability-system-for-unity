# NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::resolve_special_relation_hit/resolve_kind3_catch_relation and CombatRecordDecoder28 first-integer semantics; Unity BattleInteractionWriter shared actual owner, BattleEcsHitExecutionPlan shadow owner, exact CatchSourceSlot90 carrier; EXE B1E13AE1, closure 39DDDA15.
evidence: authority atomic relation write set and preflight frozen; CatchSourceSlot90 missing actual/shadow producer confirmed as current-content reachable first difference; CatcherSlotIndex retained only as compatibility mirror; Unity/release kind3 corpus 1/548 with zero nonzero-respond, missing action, zero action, or signed action rows; future actual+shadow package and validation matrix defined; production held; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / ATOMIC_ACTUAL_AND_SHADOW_PACKAGE_DEFINED / PRODUCTION_HELD`

Authority kind3 catch relation 的双-frame preflight、signed first action、anchor/motion、exact reciprocal slots、
respond timeout 与 target Fall 清零已闭合到 Unity owner。当前 shared actual 写 `CaughtSlotIndex` 与 compat
`CatcherSlotIndex`，但漏写 exact `CatchSourceSlot90`；HitPlan 也未捕获/投影/比较该字段。因此每次成功
kind3 relation 都会在已进入 attribution/checksum 的 exact 列产生首差。

Unity/release kind3 语料分别为 1/548；两者都没有非零 respond、缺失/零 action 或负 action，所以硬编码
300 与正 action 当前碰巧等价，但不能替代通用规则。后续
`NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001` 必须原子修改 actual 与 HitPlan shadow，并继续
dual-write compat mirror。现有 Unity runtime 验收栈未清，本轮没有脚本、content 或 Scene 修改。

## CORPUS CORRECTION（2026-09-08）

current kind3由1纠正为249且全部有catching/caught pair；exact CatchSourceSlot90 current witness显著扩大。
atomic actual+shadow与compat结论不变，以multiline总correction为准。
