# NTSD28-B3-C13-ACTIVE-WEAPON-COUNT-PLACEMENT-001 — C13 active weapon count placement

<!-- CHANGE-RECORD
id: NTSD28-B3-C13-ACTIVE-WEAPON-COUNT-PLACEMENT-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationPassPipeline.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/RandomWeapon/BattleRandomWeaponDropModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C11CandidateBuildPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C12FusionBarrierPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C13ActiveWeaponCountPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C13ActiveWeaponCountPlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C13 0x0041EF03..0x0041EF53 scans active slots after fusion and before first hit consumer, counting current definition types 1/2/4/6 into DAT_004A115C; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TEST-FIRST-RED-6 / UNITY-COMPILE-0 / FOCUSED-4-4 / C04-C13-ACTUAL-40-40 / RELATED-13-13 / WARMED-4096-0B / SELFCHECK-PASS-2026-09-05T04:11:29+08 / TARGETED-PLAY-PASS / RESULT-SHA256-2D38263658E211B69BBADCB9D08809EDF253187A347699B8E4DC9233D7123D9B / RANDOM-DROP-EXCEPTION-PRESERVED / SCENE-UNCHANGED / CONSOLE-0 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C13-PLACEMENT / EXACT-TYPE-SET / RANDOM-DROP-EXCEPTION-PRESERVED / TARGETED-PLAY-PASS`

## 改前事实与计划

- Authority在C12后、C14前扫描active slot并只计current definition type 1/2/4/6。
- Unity没有独立C13；`RunNormalDrop`在CharacterHit后临时把所有non-character计数并直接消费。
- 为尊重用户批准的随机掉武器例外，本包新增独立C13只读快照，但不让它替换drop内部现有计数、位置、RNG或生成算法。
- 先建立phase、类型集合、serial可见性及drop隔离red tests，再做最小production接线。

## 实际修改

- `BattleTickPhase.ActiveWeaponCount`加入C12 RuntimeMaintenance后、FrameAdvance serial前；full occurrence从31变为32，input-clear partial仍为4。
- `BattleRandomWeaponDropModule`新增C13 count/captured-tick carrier与capture方法；它只读active slot和current DAT type，只计`LightWeapon(1)`、`HeavyWeapon(2)`、`ThrowWeapon(4)`、`Drink(6)`。
- `SimulationPassPipeline`与`SimulationWorld`只增加该capture façade及只读诊断值；没有新增实体字段、RNG调用或结构写入。
- `RunNormalDrop`和`RunMode2Tail`正文保持不变。用户批准保留的drop仍在CharacterHit与ObjectHit之间，并继续使用其既有all-non-character门槛。
- actual sequence、C06～C12相邻phase断言更新；新增C13 focused与真实Play probe。

## 验证

| 层级 | 证据 | 结果 |
|---|---|---|
| fixture correction | 初始derived `LF2Entity`缺抽象成员；改为typed `LF2Character`测试壳 | test-only correction |
| test-first | 修正fixture后只剩phase/capture/count/tick carrier缺失 | `6 error CS` |
| Unity compile | final scripts refresh；Console精确过滤`error CS` | `0` |
| focused | job `02b8d7555ee340d287e9af6b33dc78cf` | `4/4 PASS` |
| C04～C13/actual | job `81fe373147ad4f779b3e2cc0da6d2480` | `40/40 PASS` |
| pass/module related | job `6e9b4c34a7fa4dbfa7e9ff24de72ac37` | `13/13 PASS` |
| allocation | warmed 4096 captures | `0 B` |
| SelfCheck | `Temp/NTSD_BattleRuntimeSelfCheck.result`，2026-09-05 04:11:29 +08 | `PASS` |
| targeted Play | tick6 baseline0；expected/serial/snapshot/current exact=4；captured tick6；cleanup | `PASS` |
| Play artifact | `Temp/NTSD28_B3_C13_ActiveWeaponCountPlacement.result.json` | SHA-256 `2D38263658E211B69BBADCB9D08809EDF253187A347699B8E4DC9233D7123D9B` |
| Scene/Console | Scene SHA前后`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`；目标Play后error查询 | unchanged / `0` |

## 结论与后续

C13 placement与精确类型集合已闭合。其值没有接入用户批准保留的Unity random-drop例外，因此不能把该例外误报为已对齐。下一结构首差推进到C14 type-zero hit consume相对serial remainder；hit consumer body、termination与damage均仍归B5。
