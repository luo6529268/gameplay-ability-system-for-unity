# NTSD28-B5-INPUT-HP-COST-COMPAT-STAT-RETIREMENT-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B5-INPUT-HP-COST-COMPAT-STAT-RETIREMENT-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan playable input_routing.cpp apply_action generic action transaction and Unity input compatibility call graph; EXE B1E13AE1, closure 39DDDA15.
evidence: input_routing.cpp is in both core and playable build closure and writes exact +0x34C rather than ComboCountVic. LF2Character.TryInputFrameJump has zero callers; LF2Entity.TryCharacterDatInputFrameJumpCompatibility remains reachable through the configurable LegacyCanonical resolver and shared character-DAT legacy actions. Both duplicate helpers omit multiple exact transaction fields. Replacing only the stat target would pollute exact accounting, while deleting only the writer would lose accounting on a live HP mutation. Defined one shared-transaction production package; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / COMPAT_PATH_LIVE / PARTIAL_RETIREMENT_UNSAFE / SHARED_TRANSACTION_PRODUCTION_DEFINED`

完整Authority、reachability、差异、唯一production范围与验收矩阵见同ID Task Contract。

2026-09-09后继：`NTSD28-B5-INPUT-HP-COST-COMPAT-SHARED-TRANSACTION-PRODUCTION-001 / VERIFIED`
已通过single exact core退休两个input writer；下一严格route为negative recovery owner audit。
