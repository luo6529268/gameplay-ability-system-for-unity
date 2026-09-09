# NTSD28-B3-C08-STAGE-DEPTH-PLACEMENT-001 — C08第一次stage-depth placement

<!-- CHANGE-RECORD
id: NTSD28-B3-C08-STAGE-DEPTH-PLACEMENT-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C08 lines675-681 clamp_type0_stage_depth after C07 and before C09; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-C08-AFTER-C07-BEFORE-C09 / RED-2-OF-2 / UNITY-COMPILE-0 / C04-C08-ACTUAL-27-OF-27-JOB-E2D24CD7 / STAGE-FRAME-WORKER-48-OF-48-JOB-F683EE05 / SELFCHECK-2026-09-05T02-11-19-PASS / TARGETED-PLAY-PASS / STAGE-Z-237-760 / INITIAL-Z-910 / SERIAL-OBSERVED-Z-760 / RESULT-SHA-7841FD33 / CLEANUP-PASS / SCENE-SHA-0D74E174-UNCHANGED / PLAY-EXITED / CONSOLE-0 / FULL-31-PARTIAL-4 / SECOND-C19-UNCHANGED / BEHAVIOR-PENDING-B4-B8 / NEXT-C09 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C08-PLACEMENT / TARGETED-PLAY-PASS / BEHAVIOR-PENDING-B4-B8`

## 改前事实

- Authority C08在C07后立即执行第一次type0 depth clamp，然后才C09 held refill与geometry。
- Unity现有第一次`StageBounds`在`FrameAdvance` serial remainder后；第二次在hit/catch相关阶段后。
- 现有clamp算法/ECS owner已有独立测试，本包只移动第一次调用。

## 计划

- 先写serial观察时点与双occurrence red tests。
- 移动第一次production调用，不改clamp函数或第二次调用。
- compile、focused、相关、SelfCheck与Play后记录下一首差。

## 实际改动

- 第一次`ClampCharacterZToStageBoundsAll` production调用从serial remainder后移动到C07后、serial前。
- 第二次C19 `StageBounds`调用、clamp算法、stage snapshot来源和C21 settlement均未改。
- 相邻C06/C07与actual phase断言更新；新增C08 focused与自动退出的真实Play探针。

## 验证

- red `2/2`；实现后Unity scripts compile error 0。
- C04～C08/actual `27/27` PASS，job `e2d24cd79430400d931e440934b53946`；StageBounds、
  FrameAdvance、worker相关`48/48` PASS，job `f683ee05c618455794e4dd50d75dd1d7`。
- SelfCheck `2026-09-05 02:11:19 +08:00` PASS。
- targeted Play：当前stage Z=237..760，slot50初始Z=910；tick6 serial remainder观察Z=760、调用1次，
  最终Z/ZInt均760，cleanup PASS。结果SHA
  `7841FD332C17DF4C8EE3ED2E3FEF6E9320010F19AA017AECDF6F604ECEAFD26A`。
- Scene SHA保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`；
  Play已退出，Console error 0，full/partial=31/4。

## 未关闭边界

- 本Record只证明C08 placement和single-owner，不证明现有边界数值、截断、stage snapshot来源或C21 settlement
  与authority等价；这些继续归B4/B8。
- 下一结构首差为C09 held refill相对serial remainder。
