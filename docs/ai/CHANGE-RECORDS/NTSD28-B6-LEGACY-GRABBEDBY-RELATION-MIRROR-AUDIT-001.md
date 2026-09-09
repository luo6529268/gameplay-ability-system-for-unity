# NTSD28-B6-LEGACY-GRABBEDBY-RELATION-MIRROR-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-LEGACY-GRABBEDBY-RELATION-MIRROR-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan EntityState28 canonical interaction_state/linked_child_slot/linked_parent_slot and their kind2/OPoint/held/kind5/lifecycle consumers; Unity GrabbedBy readers/writers, canonical copy, ECS Links/fingerprint and checksum/parity/raw omissions; EXE B1E13AE1, closure 39DDDA15.
evidence: authority has no second relation field; all 40 Unity references and 3 production conditional reads enumerated; current 62 OPoint-kind2 records across 39 Character sources write child -1 while shared 117 ITR-kind2 records do not write the mirror; only semantic readers are legacy raw-kind5 duplicates already routed for retirement; canonical snapshot/ECS propagation and checksum/parity blind spot confirmed; existing consumer plus nonzero-producer and user-direction carrier packages ordered; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_SECOND_RELATION_FIELD / CURRENT_OPOINT_WRITER_REACHABLE / LEGACY_READER_ROUTED / TWO_NEW_PACKAGES_PLUS_EXISTING_CONSUMER / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`

Authority的held relation只有`interaction_state + linked_child_slot + linked_parent_slot`，没有第二个
signed mirror。Unity `GrabbedBy`在current62条OPoint kind2/39个Character source上写child `-1`，
而shared117条ITR kind2又不写它；它不是exact field也不是可信compat mirror。

字段进入canonical runtime snapshot与ECS fingerprint，却不进lockstep checksum/parity/raw；两个
gameplay reader都是已路由退休的raw-kind5旧duplicate。后继先复用tracker consumer retirement，
再退休nonzero producers；carrier删除并入ReleaseTick/WeaponState/Tracker/HolderCopySlot联合schema方向。完整closure、
测量、矩阵与回滚见`docs/ai/TASKS/NTSD28-B6-LEGACY-GRABBEDBY-RELATION-MIRROR-AUDIT-001.md`。
本轮无code/content/Scene。
