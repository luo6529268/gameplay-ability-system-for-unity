# NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan EntityState28 physical slot, interaction_state/linked slots, owner_slot, battle_group, control_slot_000, score/knockout fields and playable object-spawn/hit/type3/catch consumers; Unity HolderCopySlot runtime/task writers, linked-holder/stat/type3 consumers, snapshot/ECS/checksum/parity; EXE B1E13AE1, closure 39DDDA15.
evidence: no single authority holder-copy field; 151 code-line references enumerated, 63 production across 26 files; linked-holder factory/BruteForce/HitPlan binding uses HolderCopy instead of HolderStableId; type3 actual/HitPlan still writes extra HolderCopy; legacy damage-stat readers routed for independent audit and B6 impact/held readers to existing packages; current 117 ITR-kind2, 62 OPoint-kind2 and 71 authored two-hop rows to 14 relation-capable type0 definitions measured; B5 exit/type3 completion narrowed; migration and joint schema routes frozen; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_SINGLE_AUTHORITY_FIELD / MULTIPLEXED_OWNER_CONFIRMED / B5_EXIT_CORRECTIONS_REQUIRED / CURRENT_TWO_HOP_GRAPH_WITNESS / ROUTES_SPLIT / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`

`HolderCopySlot` 是旧`f03773f3` C# parity tooling引入的multiplexed slot，不是当前2.8
Authority field。Unity用它同时表示direct holder、spawn root、legacy stat credit、type3 relation copy和
stage/self slot；Authority对应使用`linked_parent_slot`、`owner_slot`、`battle_group`、
`control_slot_000`或physical slot。

本审计更正两个旧B5范围声明：frozen linked-holder目前错读HolderCopy，type3 actual/HitPlan仍有
额外HolderCopy write；core truth table和exact group/owner/control证据保留，但family exit重开。current测量为
117 ITR-kind2、62 OPoint-kind2，以及71条通往14个kind2-capable type0 definition的authored两跳
OPoint rows。

后继legacy stats owner audit已由`NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001`闭合并回溯出
B2 AI、B3 child、B4 revival和B5 +0x2F4/stats路由。linked-holder/type3更正后复用B6
impact/held owner并退B6/B7 producers。carrier删除并入联合entity13/roster2/full20/checksum23方向。
详见两份Task Contract。本轮无code/content/Scene。

## 2026-09-09 route progress

linked-holder binding与type3 extra-write retirement已分别由
`NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001`和
`NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001`验证完成。下一严格route回到B5
`OrdinaryCreditGate2F4`/legacy stats；HolderCopy producer/carrier/schema仍受原direction gate约束。
