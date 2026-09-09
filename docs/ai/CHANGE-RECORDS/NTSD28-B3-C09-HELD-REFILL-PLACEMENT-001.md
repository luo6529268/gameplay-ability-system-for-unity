# NTSD28-B3-C09-HELD-REFILL-PLACEMENT-001 — C09第一次held-refill placement

<!-- CHANGE-RECORD
id: NTSD28-B3-C09-HELD-REFILL-PLACEMENT-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C09 settle_held_refill_objects after C08 and before C10; BattleWorld28::settle_held_refill_objects; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-C09-AFTER-C08-BEFORE-C10 / RED-2-OF-2 / UNITY-COMPILE-0 / FOCUSED-2-OF-2-JOB-E704D113 / C04-C09-ACTUAL-29-OF-29-JOB-DE5F14AC / HELD-LINK-FRAME-SNAPSHOT-WORKER-65-OF-65-JOB-CF3D09B4 / SELFCHECK-2026-09-05T02-34-48-PASS / FIRST-PLAY-FIXTURE-Y-EXPECTATION-CORRECTED-46-TO-47 / TARGETED-PLAY-PASS / SERIAL-FRAME-5 / SERIAL-POSE-95-47-199 / RESULT-SHA-62ED5E71 / CLEANUP-PASS / SCENE-SHA-0D74E174-UNCHANGED / PLAY-EXITED / CONSOLE-0 / FULL-31-PARTIAL-4 / SECOND-C20-UNCHANGED / BEHAVIOR-PENDING-B6 / NEXT-C10 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C09-PLACEMENT / TARGETED-PLAY-PASS / BEHAVIOR-PENDING-B6`

## 改前事实

- Authority C09在C08后立即扫描负relation held child，处理refill、exhaust、WPoint pose与release，然后才C10
  action snapshot/candidate geometry。
- Unity已有两次`HeldObjectProcessAll`；第一次位于serial remainder后、collision snapshot前，第二次位于命中后
  C19 clamp后。
- Unity现有owner可作为placement载体，但内部refill识别、HP/PP算术、relation清理、WPoint缺省、cover、throw/drop
  和同步RNG尚未按2.8逐项证明，本包不得宣称行为等价。

## 计划

- 先写phase、双occurrence与serial可见性red tests。
- 移动第一次production调用，不改held writer或第二次调用。
- compile、focused、相关、SelfCheck与真实Play后记录下一首差。

## 实际改动

- 第一次`HeldObjectProcessAll` production调用从serial remainder后移动到C08后、serial前。
- 第二次C20调用及`SimulationQueryAndLinkModule`、held writer、refill/WPoint/release/RNG算法均未改。
- actual及相邻C06～C08断言更新；新增C09 focused与自动退出的真实Play探针。

## 验证

- red `2/2`；实现后Unity scripts compile error 0。
- C09 `2/2` job `e704d113065e4ebfa735448230f317d7`；C04～C09/actual `29/29` job
  `de5f14ac6f3e4f5c82b0ee74082506aa`；held/link/frame/snapshot/worker `65/65` job
  `cf3d09b4c4264714bd50e424f88dcae8`。
- SelfCheck `2026-09-05 02:34:48 +08:00` PASS。
- 首次Play失败仅为fixture把既有cover=2的Y预期写成46、实际47；关键frame5已观察。修正fixture后最终Play
  tick6 serial观察frame5、pose(95,47,199)，cleanup PASS，结果SHA
  `62ED5E71452BE5FBE77EDC2197C8A57296B3A1EDE62CC9E1D006F3CA9F296E27`。
- Scene SHA保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`；
  Play已退出，Console error 0，full/partial=31/4。

## 未关闭边界

- 本Record只证明C09 placement和写入可见性，不证明现有Unity refill识别、HP/PP算术、exhaust、WPoint缺省、
  cover、throw/drop及同步RNG与authority等价；这些继续归B6。
- 下一结构首差为C10 collision-action snapshot相对serial remainder。
