# NTSD28-B6-WPOINT-DVX-EXCLUDED-GROUP-CARRIER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-WPOINT-DVX-EXCLUDED-GROUP-CARRIER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan EntityState28::object_ai_excluded_group_source_slot_2f8, BattleWorld28 held DVX writer and native_ai.cpp consumer; EXE B1E13AE1, closure 39DDDA15.
evidence: +0x2F8 defaults -1, has one held type1/4/6 writer and one non-character AI excluded-group consumer; Unity has no independent field and its SpawnerSlotIndex has broader general-spawner writers/consumers, so reuse is rejected; carrier/writer/consumer packages split; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`

Native +0x2F8不是通用spawner：它只由held type1/4/6 DVX release写holder slot，并只供
non-character AI排除该holder的battle group。Unity `SpawnerSlotIndex`拥有额外respawn/lifecycle
生产者和通用消费者，不能作为严格绑定；当前也没有独立carrier或native AI consumer。

后续按carrier→held writer→AI consumer三包实施；完整边界见
`docs/ai/TASKS/NTSD28-B6-WPOINT-DVX-EXCLUDED-GROUP-CARRIER-AUDIT-001.md`。当前runtime stack
未清，本轮无代码、content或Scene改动。

## CORPUS CORRECTION（2026-09-08）

current holder non-kind3 DVX由3纠正为184；carrier→writer→AI consumer owner顺序不变，current矩阵扩大。

## HELD RELATION DOMAIN CORRECTION（2026-09-08）

完整ITR+OPoint union将current non-kind3 DVX修正为186；原184是pickup-only。三包owner不变。

## +0x3F8 TARGET DEPENDENCY（2026-09-08）

后继`NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001`证明generic common target当前误用+0x354 owner。
本记录的+0x2F8 consumer包必须在canonical +0x3F8 carrier/producer之后实施，不得把Spawner或Owner当target。
