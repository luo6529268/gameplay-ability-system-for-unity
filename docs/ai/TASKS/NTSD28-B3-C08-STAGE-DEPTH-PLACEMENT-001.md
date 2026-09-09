# Task Contract — NTSD28-B3-C08-STAGE-DEPTH-PLACEMENT-001

> 状态：`VERIFIED / C08-PLACEMENT / TARGETED-PLAY-PASS / BEHAVIOR-PENDING-B4-B8`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C08`
> 依赖：`NTSD28-B3-C07-REVIVAL-PRODUCTION-PLACEMENT-001 / VERIFIED`

## 目标

把Unity现有第一次`ClampCharacterZToStageBoundsAll` production调用从serial remainder后移动到C07后、serial前，
对应authority C08。第二次C19 `StageBounds` occurrence保持原位。本包只闭合placement和single-owner，不修改
stage-depth clamp公式、runtime stage snapshot来源或最终C21 X/Z settlement。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs`（仅更新相邻C08 phase断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs`（仅相邻C08断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementPlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅旧phase断言必要修正）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

不修改`SimulationStageRenderModule`、ECS clamp算法、C07 revival行为、C09 held refill、C19第二次clamp、C21 settlement、
Scene/Prefab/Config/资源或authority目录。

## 不变量

- production顺序固定为C07→C08 StageBounds#1→serial remainder。
- StageBounds#1/#2仍各执行一次；本包不合并、不删除第二次C19。
- C08只处理active current-DAT type0并保持现有ECS/virtual fallback所有权。
- full occurrence仍31；input-clear partial仍4。
- stage boundary数值、截断和快照来源仍需B4/B8最终权威验证。

## 验收

1. test-first证明serial remainder观察到已夹取Z，phase位置正确且StageBounds occurrence仍为2。
2. compile0；C04～C08、StageBounds、frame/worker/snapshot回归和SelfCheck通过。
3. 真实Play定向probe通过；cleanup、Scene unchanged、Play退出、Console0。
4. 下一结构首差推进到C09 held refill对serial remainder。

## 回滚

把第一次StageBounds调用恢复到FrameAdvance后，恢复相邻phase测试；不回退C01～C07。

## 执行结果

- test-first为`2/2`预期失败：serial观察到Z=500而不是350，且phase 8仍是FrameAdvance而不是StageBounds。
- production第一次StageBounds已移动为C07→C08→serial；C19第二次occurrence未改，full/partial保持31/4。
- C04～C08/actual最终`27/27` PASS，job `e2d24cd79430400d931e440934b53946`；StageBounds、
  FrameAdvance与worker相关扩大回归`48/48` PASS，job `f683ee05c618455794e4dd50d75dd1d7`。
- Unity scripts compile error 0；完整SelfCheck `2026-09-05 02:11:19 +08:00` PASS。
- 真实production Play使用当前场景Z边界237..760：slot50从Z=910在tick6的serial remainder前被钳制到760，
  serial仅观察1次；cleanup PASS。结果SHA-256
  `7841FD332C17DF4C8EE3ED2E3FEF6E9320010F19AA017AECDF6F604ECEAFD26A`。
- Scene SHA-256保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`，
  Play已退出，Console error 0；下一结构首差为C09 held refill相对serial remainder。
