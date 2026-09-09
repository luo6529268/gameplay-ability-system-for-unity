# Task Contract — NTSD28-B3-C09-HELD-REFILL-PLACEMENT-001

> 状态：`VERIFIED / C09-PLACEMENT / TARGETED-PLAY-PASS / BEHAVIOR-PENDING-B6`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C09`
> 依赖：`NTSD28-B3-C08-STAGE-DEPTH-PLACEMENT-001 / VERIFIED`

## 目标

把Unity现有第一次`HeldObjectProcessAll` production调用从serial remainder后移动到C08后、serial前，对应
authority C09第一次`settle_held_refill_objects()`。第二次C20 `HeldProcess`保持原位。本包只闭合placement、
single-owner及C09写入对随后serial的可见性，不改现有held/refill/WPoint/RNG算法。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs`（仅更新相邻C09断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs`（仅更新相邻C09断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs`（仅更新相邻C09断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementPlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅旧phase断言必要修正）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

不修改`SimulationQueryAndLinkModule`、`BattleHeldObjectWriter`、`LF2WeaponHeldStateResolver`、held/refill数值、
WPoint同步、RNG callsite、第二次C20 pass、C10 snapshot/candidate、Scene/Prefab/Config/资源或authority目录。

## 不变量

- production顺序固定为C08 StageBounds#1→C09 HeldProcess#1→serial remainder。
- HeldProcess#1/#2仍各执行一次；本包不合并、不删除第二次C20。
- 现有Unity refill分类、HP/PP算术、exhaust、WPoint、throw/drop与RNG行为继续标为B6 pending。
- full occurrence仍31；input-clear partial仍4。
- C09后仍存在Unity serial remainder再到C10 snapshot的结构差，下一包继续处理。

## 验收

1. test-first证明serial remainder观察到C09 held写入，phase位置正确且HeldProcess occurrence仍为2。
2. compile0；C04～C09、held/link、frame/worker/snapshot回归和SelfCheck通过。
3. 真实Play定向probe通过；cleanup、Scene unchanged、Play退出、Console0。
4. 下一结构首差推进到C10 collision-action snapshot相对serial remainder。

## 回滚

把第一次HeldProcess调用恢复到FrameAdvance后，恢复相邻phase测试；不回退C01～C08。

## 执行结果

- test-first `2/2`预期失败：serial观察held frame0而不是5，phase 9为FrameAdvance而不是HeldProcess。
- 第一次HeldProcess已移动为C08→C09→serial；C20第二次occurrence未改，full/partial保持31/4。
- C09 focused `2/2` PASS，job `e704d113065e4ebfa735448230f317d7`；C04～C09/actual
  `29/29` PASS，job `de5f14ac6f3e4f5c82b0ee74082506aa`；held/link/frame/snapshot/worker
  `65/65` PASS，job `cf3d09b4c4264714bd50e424f88dcae8`。
- Unity scripts compile error 0；完整SelfCheck `2026-09-05 02:34:48 +08:00` PASS。
- 首次Play关键frame时点已通过，但探针把既有Unity cover=2的Y预期误写46、实际47；只修正fixture，未改生产算法。
  最终Play中slot50/51在tick6让serial观察frame5、pose(95,47,199)，cleanup PASS；结果SHA-256
  `62ED5E71452BE5FBE77EDC2197C8A57296B3A1EDE62CC9E1D006F3CA9F296E27`。
- Scene SHA-256保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`，
  Play已退出，Console error 0；下一结构首差为C10 collision-action snapshot相对serial remainder。
