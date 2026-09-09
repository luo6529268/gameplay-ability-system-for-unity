# NTSD28-B3-C10-COLLISION-ACTION-SNAPSHOT-PLACEMENT-001 — C10碰撞动作快照placement

<!-- CHANGE-RECORD
id: NTSD28-B3-C10-COLLISION-ACTION-SNAPSHOT-PLACEMENT-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CooldownWriterExtractionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C10 snapshot_actions after C09 and before C11; BattleWorld28::snapshot_actions copies frame.action to tick_action_snapshot; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-C10-AFTER-C09-BEFORE-C11 / RED-2-OF-3 / UNITY-COMPILE-0 / FOCUSED-3-OF-3-JOB-9F551EDC / FIRST-C04-C10-29-OF-32-OLD-ADJACENCY-ASSERTIONS / FINAL-C04-C10-32-OF-32-JOB-90DE21FC / SNAPSHOT-REST-FRAME-WORKER-59-OF-59-JOB-6871E19D / SELFCHECK-2026-09-05T02-51-04-PASS / TARGETED-PLAY-PASS / CURRENT-FRAME-5 / C10-SNAPSHOT-0 / SERIAL-REST-3 / FINAL-REST-0 / RESULT-SHA-D96A619A / CLEANUP-PASS / SCENE-SHA-0D74E174-UNCHANGED / PLAY-EXITED / CONSOLE-0 / FULL-31-PARTIAL-4 / NEXT-C11 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C10-SNAPSHOT-PLACEMENT / C11-REST-NOT-EARLY / TARGETED-PLAY-PASS`

## 改前事实

- Authority C10只按live slot冻结当前action；C11随后才进入候选构建相关处理。
- Unity production在serial remainder后调用`CaptureCollisionFrameSnapshotsAll`，且该入口先执行
  `PrepareAttackerRestForCandidateAll`再冻结`Frame.N→Prev2/Runtime.PrevFrame2`。
- 因而C10 placement晚，且若机械整体前移会把C11 rest prelude一并提前；必须拆开职责。

## 计划

- test-first覆盖snapshot-before-serial、rest-not-early及phase位置。
- 新增snapshot-only world seam；public combined direct入口保持兼容。
- production在C09后执行snapshot-only，serial后单独执行rest prelude再进入C11相关链。

## 实际改动

- `CaptureCollisionActionSnapshotsOnlyAll`承接C10纯快照；既有public combined入口继续先rest再调用snapshot-only，
  保持direct兼容。
- production将`CollisionSnapshot`移到C09后、serial前；`PrepareAttackerRestForCandidateAll`显式留在serial后、
  PairVRest/candidate前，不新增phase occurrence。
- actual与相邻C06～C09断言更新；新增C10 focused与自动退出的真实Play探针。

## 验证

- red `2/3`；实现后compile error 0；focused `3/3` job `9f551edced964e278da247eda45fa1bd`。
- C04～C10首次29/32只失败于C06～C08旧相邻断言；final `32/32` job
  `90de21fc13fd426280ba6368b12eb35b`。
- cooldown/collision snapshot/frame/worker/state snapshot `59/59` job
  `6871e19d9b7a497cb3e7118f1a9aee54`；SelfCheck `2026-09-05 02:51:04 +08:00` PASS。
- targeted Play tick6：serial把current frame改5，C10冻结值及Runtime镜像保持0；serial仍看到rest3，随后C11清0；
  cleanup PASS，SHA `D96A619A06CE542210F6BF75B002C90F3FB92FF65C22CB703252DB6222D54FA0`。
- Scene SHA保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`；
  Play已退出、Console error 0，full/partial=31/4。

## 未关闭边界

- 本Record不证明C11 pair-vrest/candidate geometry/pass-shape及snapshot消费者完全等价；下一包继续闭合C11。

## 后续修正

- `NTSD28-B3-C11-CANDIDATE-BUILD-PLACEMENT-001`已将C11 rest/pair/candidate transaction移动到serial前。
  本Record验证的C10 snapshot-only位置仍有效；“C11 rest暂留serial后”是已被后续包取代的阶段性边界。
