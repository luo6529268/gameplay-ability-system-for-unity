# Task Contract — NTSD28-B3-C13-ACTIVE-WEAPON-COUNT-PLACEMENT-001

> 状态：`VERIFIED / C13-PLACEMENT / EXACT-TYPE-SET / RANDOM-DROP-EXCEPTION-PRESERVED / TARGETED-PLAY-PASS`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C13`
> 依赖：`NTSD28-B3-C12-FUSION-BARRIER-PLACEMENT-001 / VERIFIED`

## 目标

在C12 fusion后、serial remainder前建立独立C13 active weapon count barrier，按当前DAT定义类型只统计active type 1/2/4/6，并提供本tick只读快照。

本包不把该快照接入用户批准保留的Unity随机掉武器例外。现有drop位置、RNG、候选表、所有非character计数门槛和生成行为保持不变。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationPassPipeline.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/RandomWeapon/BattleRandomWeaponDropModule.cs`（只新增C13 snapshot owner；不改RunNormalDrop/RunMode2Tail）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs`～`NTSD28C12FusionBarrierPlacementEditorTests.cs`（仅相邻phase index/count断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C13ActiveWeaponCountPlacementEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C13ActiveWeaponCountPlacementPlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅phase契约必要修正）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

不修改随机掉武器例外的生产算法、hit/catch算法、entity lifecycle、DAT/Config、Scene/Prefab或authority目录。

## 不变量与验收

- production顺序为C11 candidate→C12 fusion→C13 active weapon count→serial remainder。
- C13扫描active runtime slots，读取当前DAT定义类型，且只计1/2/4/6；不消费RNG、不生成/销毁实体、不写实体字段。
- C13值在同tick serial remainder中可见；serial中的后续变化不能反写本tick快照。
- `BattleRandomWeaponDropModule.RunNormalDrop`与`RunMode2Tail`保持原代码和用户例外语义。
- full/partial occurrence预期为32/4；compile、focused、相关、SelfCheck、Play和Scene/Console门通过。
- 下一结构首差推进到C14 type-zero hit consume相对serial remainder。

## 回滚

删除C13 phase、snapshot façade与测试；恢复C12→serial。不得回退C01～C12。

## 完成证据

- test-first red在fixture抽象壳修正后稳定为6个C13缺失能力编译错误：缺少phase、capture入口、count与captured-tick carrier。
- production新增C13只读快照：扫描active runtime slots，按current DAT type只计1/2/4/6，位于C12后、serial前；4096次warmed capture分配0 B。
- `RunNormalDrop`与`RunMode2Tail`正文未改；focused证明4个type5仍按用户例外的旧all-non-character gate阻止drop且不消费RNG。
- Unity fresh compile为0个`error CS`；final C13 focused job `02b8d7555ee340d287e9af6b33dc78cf`为4/4 PASS。
- final C04～C13/actual sequence job `81fe373147ad4f779b3e2cc0da6d2480`为40/40 PASS；pass-order/module相关job `6e9b4c34a7fa4dbfa7e9ff24de72ac37`为13/13 PASS。
- fresh `BattleRuntimeSelfCheck`于2026-09-05 04:11:29 +08写出PASS；full/partial occurrence为32/4。
- 最终真实`NTSD_Battle` Play probe：tick6，baseline0，注入current-DAT type0～6后expected/serial/final snapshot/current exact均为4，captured tick=6，cleanup PASS。结果SHA-256为`2D38263658E211B69BBADCB9D08809EDF253187A347699B8E4DC9233D7123D9B`。
- 最终Play已退出、Console error为0；Scene SHA-256前后均为`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`。
- 下一结构首差为C14 type-zero hit consume相对serial remainder。
