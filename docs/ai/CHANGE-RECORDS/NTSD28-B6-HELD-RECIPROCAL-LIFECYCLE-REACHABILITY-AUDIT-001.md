# NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::despawn/clear_entity_links and held invalid reciprocal handling; Unity SimulationRegistryModule slot release/reuse path; EXE B1E13AE1, closure 39DDDA15.
evidence: Authority atomically clears held/catch reverse references before slot reset; Unity releases slot/generation without scanning related active entities and can reuse the slot in-tick, leaving invalid negative child cleanup to C09/C20 and permitting an ABA window; lifecycle reachability is structurally confirmed, full exact-field owner audit required before production; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / LIFECYCLE_REACHABILITY_CONFIRMED / ATOMIC_DESPAWN_LINK_CLEANUP_REQUIRED / PRODUCTION_HELD`

Authority在despawn释放slot前原子清全部held/catch反向引用；Unity registry直接release并允许
同tick reuse，没有对应scan。invalid child因此是Unity lifecycle缺口的下游症状，且存在physical-slot
ABA窗口。前一`DYNAMIC_REACHABILITY_PENDING`已被本记录解决。

下一步必须先做`NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001`，冻结held/catch exact
字段与structural owner，再按cleanup→invalid handler顺序实施。完整证据见
`docs/ai/TASKS/NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001.md`；本轮无代码。
