# NTSD28-B3-C23-C24-WORLD-CLOCK-001 — native world clock

<!-- CHANGE-RECORD
id: NTSD28-B3-C23-C24-WORLD-CLOCK-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Runtime/NTSD28NativeWorldClockState.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C11CandidateBuildPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C12FusionBarrierPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C13ActiveWeaponCountPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C14TypeZeroHitPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C14TypeZeroHitPlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28ResidualSerialTailRehomeEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28ResidualSerialTailRehomePlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C21C22PlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C21C22PlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleStateSnapshotRestoreEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan BattleWorld28 begin_native_resource_tick increments mod12/mod3, then begin_frame_tick increments sequence once before C25; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TEST-FIRST-COMPILE-RED-20 / UNITY-COMPILE-0 / FOCUSED-14-14 / SNAPSHOT-CHECKSUM-78-78 / C04-C24-ACTUAL-51-51 / SELFCHECK-PASS-2026-09-05T06:01:19+08 / TARGETED-PLAY-PASS / RESULT-SHA256-186F7F0243538008F1D7B74960A736DC650239E632C7FB5B161E2E4C818D05F3 / SCENE-UNCHANGED / CONSOLE-0 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C23-C24-SINGLE-OWNER / SNAPSHOT-CHECKSUM-CLOSED / TARGETED-PLAY-PASS / C25-PENDING`

## 改前事实与计划

- Authority C23执行`phase12=(phase12+1)%12`与`phase3=(phase3+1)%3`；C24执行`++sequence`，随后C25消费。
- Unity只有C00阶段写入的`CurrentTickIndex/FrameMod12/FrameToggle`，没有独立、正确可见性边界的C23/C24状态。
- 新状态必须进入reset、snapshot/restore、checksum/parity；先写red，再实现single owner和生产phase。

## 实际修改

- 新增Unity-owned `NTSD28NativeWorldClockState`，字段精确为`ResourcePhase12`、`ResourcePhase3`、`FrameSequence`，Reset归零。
- `SimulationWorld.BeginNativeResourceTick()`按Authority执行`(+1)%12`/`(+1)%3`；`BeginNativeFrameTick()`执行`++sequence`。
- TickSystem在C22后依次记录/执行`NativeResourceTick`、`NativeFrameTick`，再进入临时serial proxy；full/partial=34/4。
- core snapshot schema 5→6、full snapshot 7→8、checksum schema 10→11；capture、restore、checksum和parity JSON都纳入三字段。
- 共享Server package未改；现有`FrameMod12/CurrentTickIndex/FrameToggle`语义未改；C25 consumers未提前实现。

## 验证

| 层级 | 证据 | 结果 |
|---|---|---|
| test-first | fresh compile diagnostics | `20 error CS`，缺失clock/phase/API |
| Unity compile | final refresh | `0 error CS` |
| focused/schema | job `3ba28aa75dcb4f3e81da495ae657846c` | `14/14 PASS` |
| snapshot/restore/checksum/lockstep | job `18c0280d17a14490ad9603f26a5a57a5` | `78/78 PASS` |
| C04～C24/actual | job `8d4d747fa977495c8b838832fdc1142d` | `51/51 PASS` |
| SelfCheck | 2026-09-05 06:01:19 +08 | `PASS` |
| targeted Play | tick6，5→6 / 2→0 / 5→6，phase24～27正确 | `PASS` |
| artifact | `Temp/NTSD28_B3_C23_C24_WorldClock.result.json` | SHA-256 `186F7F0243538008F1D7B74960A736DC650239E632C7FB5B161E2E4C818D05F3` |
| Scene/Console | SHA/mtime unchanged；Play exited | `0 error` |

## 失败与修正留痕

- 首轮73项state闭包job `6eb164c44a0d419795a194d395c5a6c6`仅旧core schema `5`断言失败；审计后确认full snapshot/checksum格式也有意变化，统一升为6/8/11。
- 第二轮73项job `fd94574064c9458bbb89d0dcedd1f1b7`仅两个历史carrier测试仍期待7/10；只更新schema断言后，最终扩展78项全部通过。

## 结论与后续

C23/C24 single owner和持久化/校验闭包已完成。它们目前只提供权威时钟，不代表C25资源或frame行为已对齐；下一包必须从C25现有writer inventory和逐槽动态遍历骨架开始，禁止把临时serial或LateEntityUpdate全局scan直接标成C25。
