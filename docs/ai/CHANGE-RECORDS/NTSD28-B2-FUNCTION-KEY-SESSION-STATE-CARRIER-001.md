# NTSD28-B2-FUNCTION-KEY-SESSION-STATE-CARRIER-001 — Session状态与event-byte载体

<!-- CHANGE-RECORD
id: NTSD28-B2-FUNCTION-KEY-SESSION-STATE-CARRIER-001
status: FOCUSED_TEST_PASS
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeFunctionKeySessionState.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleStateSnapshotRestoreEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
authority: NTSD 2.8-Logan playable live native function-key runtime/session/queue/dispatch closure and function-key crosswalk manifest.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST-50-CS0246-CS0103-CS1061 / MASK-0XF4 / QUEUE-BIT-FOLD / FIXED-F3-F6-F7-F8-F9-DISPATCH / SESSION-GATE-ORDER / F3-ONE-WAY-LOCK / F6-F9-COUNTS-AND-PENDING / SAME-WINDOW-F9-WINS / DIRECT-LAST-ACCEPTED-WINS / CONSUME-HANDOFF / BATTLE-RUNTIME-RESET / CORE-SNAPSHOT-5 / FULL-SNAPSHOT-7 / CHECKSUM-10 / FULL-RESTORE / WARM-4096-ZERO-ALLOC / COMPILE-0 / FOCUSED-9-OF-9-387B1A25 / RELATED-65-OF-65-ED898030 / FULL-SELFCHECK-2026-09-04-201923-PASS / EXPECTED-NEGATIVE-7-REVIEWED / CONSOLE-0 / LEDGER-155-110-PASS / PHYSICAL-AND-EFFECTS-UNCONNECTED / AUTHORITY-READ-ONLY
-->

> 状态：`FOCUSED_TEST_PASS / SESSION-CARRIER-READY / SNAPSHOT-CHECKSUM-RESTORE-READY / PRODUCTION-UNCONNECTED / NEXT-PRODUCTION-INTEGRATION`

## 改前职责

- pure route可分类F3/F6～F9，但BattleRuntime没有Authority Session state/event byte载体。
- snapshot/checksum仍只覆盖旧`InitStatsRequest/Mode2Request`，旧F7/F8/F9语义不能复用。

## 目标职责

- 新carrier唯一拥有queue、fixed dispatch、acceptance state、counts与pending handoff。
- BattleRuntime reset、core/full snapshot restore与checksum完整覆盖全部确定性字段。
- production physical/effects继续不接，下一包再做统一integration。

## 实际改动

- 新增Session carrier：mask/queue/dispatch/apply/reset/count/pending/consume/restore全部为明确值字段。
- `BattleRuntimeState`新增nonserialized carrier root并在Reset时重建/复位。
- core/full snapshot与restore覆盖10个字段；checksum按相同语义逐字段写入；schema `4→5 / 6→7 / 9→10`。
- focused tests新增9项；完整restore test加入carrier roundtrip；两个旧schema expectation仅同步新版本。
- `SimulationTickDriver`、physical latch、旧F7/F8/F9 effect、Config/Scene/Authority均未修改。

## 验证计划

1. 新tests先取missing-type/schema red。
2. 最小实现carrier/root/snapshot/restore/checksum与schema bump。
3. focused+相关回归、full SelfCheck、Console、validator。

## 实际验证

- red：50条预期missing-type/root/snapshot错误。
- compile：Unity refresh/domain reload后0 error。
- focused：`387b1a255c834421bc3d8ac29d14de45 = 9/9`。
- final related：`ed8980300085420fab8edfb17e10c509 = 65/65`，覆盖router/carrier、old FunctionKey、B1 Host、
  core/full snapshot、restore、checksum/ring及旧carrier schema。
- full SelfCheck：20:19:23 `PASS`；7条既有negative fixture日志审阅后清空，Console 0。
- validator：`PASSED / Records 155 / governed code files 110`。

## 未验证与边界

- carrier尚无production caller；F3/F6～F12物理行为与effects仍未运行。
- 下一包只能做统一production integration和typed handoff；F7/F8/F9真正post-tick effect仍受B3/B8边界约束。

## 回滚

按Task合同删除carrier与字段，schema恢复4/6/9；production未接。
