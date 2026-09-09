# NTSD28-B2-RNG-WORLD-STATE-001 — native dual RNG world state

<!-- CHANGE-RECORD
id: NTSD28-B2-RNG-WORLD-STATE-001
status: FOCUSED_TEST_PASS
change-kind: SCRIPT_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/InProcessBattleWorldBootstrap.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomWorldStateEditorTests.cs
authority: NTSD 2.8-Logan NativeRandom28 lifecycle/state plus current Unity world reset, MatchConfig, lockstep bootstrap, snapshot/restore and checksum contracts.
evidence: TEST-FIRST-SCALAR-TYPE-MISSING / UNITY-COMPILE-0 / NEW-5-5 / INITIAL-RELATED-38-PASS-7-FAIL / TABLESEED-FIRST-DIFFERENCE-CORRECTED / FINAL-RELATED-50-50 / CORE-CAPTURE-1024-ZERO-ALLOC / WARM-RESTORE-128-ZERO-ALLOC / CHECKSUM-256-ZERO-ALLOC / FRESH-WORLD-RESTORE-PASS / FULL-SELFCHECK-PASS / CONSOLE-0 / LEGACY-RNG-PRESERVED / CONSUMERS-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / WORLD_STATE_READY / CONSUMERS_UNMIGRATED`

## 改前事实

- 独立2.8双流primitive已通过6/6+related4/4，但没有world owner。
- world/reset/MatchConfig/bootstrap/snapshot/checksum只处理legacy `DeterministicRng`。
- core scalar capture与runtime checksum当前均有warm allocation-free合同。

## 改后职责

world拥有并按相同match seed管理2.8双流；snapshot/checksum覆盖完整scalar状态与table hash，且不复制表。
现有consumer仍只用legacy RNG，迁移留独立包。

## 验证记录

- test-first缺失scalar type red；新focused `13d8...d71c6` 5/5。
- 初次related `bc37...b1fd0`：38 pass/7 snapshot restore fail，定位default table seed provenance缺失；
  新增tableSeed标量后final `9c40...1509` 50/50 PASS。
- core capture 1024、warm restore 128、runtime checksum 256循环均0 managed allocation；fresh transfer通过。
- full SelfCheck最终PASS；compile/Console0。consumer、B0 exporter/JSON trace未迁移。
- Ledger/diff在最终文档后重跑。
