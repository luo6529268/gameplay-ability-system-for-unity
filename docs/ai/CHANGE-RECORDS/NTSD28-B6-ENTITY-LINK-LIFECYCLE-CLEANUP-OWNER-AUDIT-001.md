# NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::despawn/clear_entity_links exact held and catch fields; Unity SimulationRegistryModule release transaction and relation stores; EXE B1E13AE1, closure 39DDDA15.
evidence: exact mappings frozen for LinkState/TargetSlotIndex/HolderStableId/CaughtSlotIndex/CatchSourceSlot90/CaughtDuration with explicit compatibility mirrors; atomic post-success pre-return cleanup owner defined; Kind4 count and HolderCopy exclusions retained; lifecycle cleanup and invalid-preserve production packages split; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / TWO_PRODUCTION_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`

Authority despawn held/catch字段已映射到Unity exact runtime载体；`CatcherSlotIndex`、
`HeldWeaponStableId`只作为当前compat mirror同步清理，`HolderCopySlot`与`Kind4SourceCount92`明确保留。
cleanup由registry release事务唯一调用，在slot release成功后、返回/再分配前无间隙完成。

后续先实现`NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001`，获得runtime绿灯后再实现
`NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001`。完整矩阵见
`docs/ai/TASKS/NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001.md`；本轮无code。
