# NTSD28-B6-LEGACY-TRACKER-RELATION-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-LEGACY-TRACKER-RELATION-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan OPoint kind2 reciprocal relation, native kind5 substitution and type3 linked-parent ownership; Unity TrackerFlag/TrackerParent producers, legacy readers, ECS/canonical copy/base-shell snapshot; EXE B1E13AE1, closure 39DDDA15.
evidence: authority has no tracker layer beyond reciprocal links; two Unity factories write current-reachable TrackerFlag/TrackerParent for 62 OPoint kind2 records/56 edges; formal kind5 shared consumer already bypasses legacy raw readers and current 13 OPoint target OIDs have zero kind5 ITR intersection; TrackerFlag checksum omission and duplicate TrackerParent snapshot handle identified; producer/consumer/carrier packages split with joint schema direction gate; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_TRACKER_LAYER / CURRENT_OPOINT_WRITERS_REACHABLE / LEGACY_READERS_DEFAULT_BYPASSED / THREE_PACKAGE_SPLIT_DEFINED / SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`

Authority OPoint kind2、kind5与type3 owner只使用reciprocal link，不存在TrackerFlag/managed parent第二层。Unity两个
factory会为current62条OPoint kind2写1/-1与对象cache；正式kind5已由shared resolver按link处理，旧raw readers
默认被绕过。current OPoint target与353条kind5的交集为0，但extra fingerprint/snapshot状态立即可达。

后继按producer retirement→legacy consumer retirement→用户方向后的联合carrier/schema migration推进；
联合迁移已扩大为ReleaseTick/WeaponState/Tracker/GrabbedBy/HolderCopySlot。完整closure与
矩阵见`docs/ai/TASKS/NTSD28-B6-LEGACY-TRACKER-RELATION-OWNER-AUDIT-001.md`。当前runtime栈未清，本轮无code/content/Scene。
